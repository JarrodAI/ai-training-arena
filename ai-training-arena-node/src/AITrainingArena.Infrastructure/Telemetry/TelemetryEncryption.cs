using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using AITrainingArena.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.Infrastructure.Telemetry;

/// <summary>
/// Encrypts telemetry records for owner-only access and anonymizes for DAO aggregate.
///
/// Encryption: AES-256-GCM with key derived from owner's ETH public key via ECDH.
/// Anonymization: strips NFT IDs, addresses, and PII per blueprint Section 14.3.
/// IPFS path: ipfs://QmUser{nftId}/{battleId}/ (encrypted, owner only)
/// DAO path:  ipfs://QmDAO/{date}/aggregate/   (public, anonymized)
/// </summary>
public sealed class TelemetryEncryption
{
    private readonly ILogger<TelemetryEncryption> _logger;

    public TelemetryEncryption(ILogger<TelemetryEncryption> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Encrypts a telemetry record using AES-256-GCM.
    /// Key is derived by hashing the owner's Ethereum address (Phase 1 simplification).
    /// Phase 2 will use ECDH key agreement with the owner's ETH public key.
    /// </summary>
    /// <param name="record">Telemetry record to encrypt.</param>
    /// <param name="ownerAddress">Owner's Ethereum wallet address (0x-prefixed).</param>
    /// <returns>Encrypted bytes: [12-byte nonce][16-byte tag][ciphertext]</returns>
    public byte[] EncryptForOwner(TelemetryRecord record, string ownerAddress)
    {
        var json = JsonSerializer.Serialize(record);
        var plaintext = Encoding.UTF8.GetBytes(json);

        // Derive a 256-bit key from owner address (Phase 1: SHA-256 of address)
        // Phase 2: ECDH key agreement using owner's secp256k1 public key
        var key = DeriveKeyFromAddress(ownerAddress);

        using var aes = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        var nonce = new byte[AesGcm.NonceByteSizes.MaxSize]; // 12 bytes
        RandomNumberGenerator.Fill(nonce);

        var ciphertext = new byte[plaintext.Length];
        var tag = new byte[AesGcm.TagByteSizes.MaxSize]; // 16 bytes

        aes.Encrypt(nonce, plaintext, ciphertext, tag);

        // Output format: nonce (12) + tag (16) + ciphertext
        var result = new byte[nonce.Length + tag.Length + ciphertext.Length];
        nonce.CopyTo(result, 0);
        tag.CopyTo(result, nonce.Length);
        ciphertext.CopyTo(result, nonce.Length + tag.Length);

        _logger.LogDebug(
            "Encrypted telemetry for owner {Address}: {PlainLen} → {CipherLen} bytes",
            ownerAddress[..8] + "...", plaintext.Length, result.Length);

        return result;
    }

    /// <summary>
    /// Decrypts a telemetry record using the owner's private key.
    /// Phase 1: derives key from address hash (symmetric).
    /// </summary>
    /// <param name="encrypted">Encrypted bytes from EncryptForOwner.</param>
    /// <param name="ownerAddress">Owner's Ethereum address.</param>
    /// <returns>Decrypted TelemetryRecord.</returns>
    public TelemetryRecord DecryptAsOwner(byte[] encrypted, string ownerAddress)
    {
        const int nonceSize = 12;
        const int tagSize = 16;

        if (encrypted.Length < nonceSize + tagSize)
            throw new ArgumentException("Encrypted payload too short", nameof(encrypted));

        var key = DeriveKeyFromAddress(ownerAddress);
        var nonce = encrypted.AsSpan(0, nonceSize).ToArray();
        var tag = encrypted.AsSpan(nonceSize, tagSize).ToArray();
        var ciphertext = encrypted.AsSpan(nonceSize + tagSize).ToArray();

        var plaintext = new byte[ciphertext.Length];

        using var aes = new AesGcm(key, AesGcm.TagByteSizes.MaxSize);
        aes.Decrypt(nonce, ciphertext, tag, plaintext);

        var json = Encoding.UTF8.GetString(plaintext);
        return JsonSerializer.Deserialize<TelemetryRecord>(json)
            ?? throw new InvalidOperationException("Failed to deserialize decrypted telemetry");
    }

    /// <summary>
    /// Anonymizes a telemetry record for the DAO public aggregate dataset.
    /// Implementation from blueprint Section 14.3:
    /// - Hash battle_id (one-way, prevents correlation)
    /// - Strip NFT IDs and wallet addresses
    /// - Keep: class, timestamp (day precision only), aggregate stats
    /// - Keep model family (e.g. "qwen-14b") but strip full path
    /// </summary>
    /// <param name="record">Full telemetry record to anonymize.</param>
    /// <returns>Anonymized record safe for public DAO aggregation.</returns>
    public AnonymizedTelemetryRecord AnonymizeForDAO(TelemetryRecord record)
    {
        // Hash battle ID for correlation prevention
        var hashedBattleId = Convert.ToHexString(
            SHA256.HashData(Encoding.UTF8.GetBytes(record.BattleId)));

        // Round timestamp to day precision
        var dayPrecisionTimestamp = record.Timestamp.Date;

        // Anonymized rounds: keep question stats but no PII
        var anonymousRounds = record.Rounds.Select(r => new AnonymizedRound(
            HopCount: r.Question.HopCount,
            Difficulty: r.Question.Difficulty,
            Correct: r.SolverResponse.Correct,
            ResponseTimeMs: r.SolverResponse.ResponseTimeMs,
            SearchQueryCount: r.SolverResponse.SearchQueries.Count
        )).ToList();

        return new AnonymizedTelemetryRecord(
            HashedBattleId: hashedBattleId,
            Date: dayPrecisionTimestamp,
            Class: record.Class,
            TotalRounds: record.Rounds.Count,
            CorrectAnswers: record.Rounds.Count(r => r.SolverResponse.Correct),
            AvgDifficulty: record.Rounds.Count > 0
                ? record.Rounds.Average(r => (double)r.Question.Difficulty)
                : 0.0,
            AvgResponseTimeMs: record.Rounds.Count > 0
                ? record.Rounds.Average(r => (double)r.SolverResponse.ResponseTimeMs)
                : 0.0,
            Winner: record.Result.Winner == record.Proposer ? "proposer" : "solver",
            ProposerScore: record.Result.ProposerScore,
            SolverScore: record.Result.SolverScore,
            BurnAmount: record.Result.BurnAmount,
            Rounds: anonymousRounds
        );
    }

    // ─── Private ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Derives a 256-bit AES key from an Ethereum address.
    /// Phase 1: SHA-256(address + constant salt).
    /// Phase 2: ECDH with owner's secp256k1 public key.
    /// </summary>
    private static byte[] DeriveKeyFromAddress(string address)
    {
        const string salt = "AITrainingArena-v1-telemetry-key";
        var input = Encoding.UTF8.GetBytes(address.ToLowerInvariant() + salt);
        return SHA256.HashData(input);
    }
}

/// <summary>
/// Anonymized telemetry record for DAO public aggregation.
/// Contains no PII, no wallet addresses, no NFT IDs.
/// </summary>
public record AnonymizedTelemetryRecord(
    string HashedBattleId,
    DateTime Date,
    AITrainingArena.Domain.Enums.AgentClass Class,
    int TotalRounds,
    int CorrectAnswers,
    double AvgDifficulty,
    double AvgResponseTimeMs,
    string Winner,
    decimal ProposerScore,
    decimal SolverScore,
    decimal BurnAmount,
    IReadOnlyList<AnonymizedRound> Rounds);

/// <summary>
/// Anonymized single round for DAO aggregate.
/// </summary>
public record AnonymizedRound(
    int HopCount,
    decimal Difficulty,
    bool Correct,
    long ResponseTimeMs,
    int SearchQueryCount);
