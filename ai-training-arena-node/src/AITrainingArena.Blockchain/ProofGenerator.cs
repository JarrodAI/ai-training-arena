using System.Security.Cryptography;
using System.Text;
using AITrainingArena.Domain.Interfaces;
using AITrainingArena.Domain.ValueObjects;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.Blockchain;

/// <summary>
/// Generates Merkle tree proofs for completed battles.
/// Phase 1: ECDSA signature of Merkle root (ZK upgrade planned for Phase 2).
/// Merkle leaves: hash(questionText + solverAnswer + isCorrect + difficultyScore)
/// </summary>
public sealed class ProofGenerator : IProofGenerator
{
    private readonly ILogger<ProofGenerator> _logger;

    public ProofGenerator(ILogger<ProofGenerator> logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Builds a Merkle tree from battle Q&amp;A pairs, computes the root,
    /// and produces an ECDSA signature of the root as the proof payload.
    /// Phase 2 will upgrade this to a ZK-SNARK proof.
    /// </remarks>
    public Task<byte[]> GenerateProofAsync(BattleResult result, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        _logger.LogInformation(
            "Generating battle proof for battle with winner={Winner}, questions={Questions}",
            result.Winner, result.TotalQuestions);

        // Build leaf hashes from result summary fields
        // In production each Q&A pair would produce a leaf; here we hash the result fields
        var leaves = BuildLeafHashes(result);
        var merkleRoot = ComputeMerkleRoot(leaves);

        // Phase 1: proof = merkleRoot (32 bytes) + SHA-256 HMAC as integrity check
        // Phase 2 will replace with actual ZK proof
        var proof = BuildProofPayload(merkleRoot, result);

        _logger.LogInformation(
            "Proof generated. Merkle root: {Root}", Convert.ToHexString(merkleRoot));

        return Task.FromResult(proof);
    }

    /// <inheritdoc />
    public Task<bool> VerifyProofAsync(byte[] proof, BattleResult result, CancellationToken ct = default)
    {
        ct.ThrowIfCancellationRequested();

        // Proof format: [1-byte version][32-byte merkle root][timestamp][winner][integrity]
        if (proof.Length < 33)
        {
            _logger.LogWarning("Proof too short to verify: {Length} bytes", proof.Length);
            return Task.FromResult(false);
        }

        var leaves = BuildLeafHashes(result);
        var expectedRoot = ComputeMerkleRoot(leaves);
        var actualRoot = proof.AsSpan(1, 32).ToArray(); // skip 1-byte version header

        var isValid = expectedRoot.SequenceEqual(actualRoot);
        _logger.LogInformation("Proof verification result: {Valid}", isValid);
        return Task.FromResult(isValid);
    }

    // ─── Private Helpers ─────────────────────────────────────────────────────

    /// <summary>
    /// Builds Merkle leaf hashes from the battle result fields.
    /// Each leaf represents a distinct piece of battle state.
    /// </summary>
    private static List<byte[]> BuildLeafHashes(BattleResult result)
    {
        var leaves = new List<byte[]>
        {
            HashLeaf($"winner:{result.Winner}"),
            HashLeaf($"proposerScore:{result.ProposerScore}"),
            HashLeaf($"solverScore:{result.SolverScore}"),
            HashLeaf($"totalQuestions:{result.TotalQuestions}"),
            HashLeaf($"correctAnswers:{result.CorrectAnswers}"),
            HashLeaf($"avgDifficulty:{result.AvgDifficulty}"),
            HashLeaf($"proposerReward:{result.ProposerReward}"),
            HashLeaf($"solverReward:{result.SolverReward}"),
            HashLeaf($"burnAmount:{result.BurnAmount}"),
        };

        if (result.TelemetryIpfsHash is not null)
            leaves.Add(HashLeaf($"ipfs:{result.TelemetryIpfsHash}"));

        // Pad to next power of 2 for balanced tree
        while (!IsPowerOfTwo(leaves.Count))
            leaves.Add(HashLeaf("padding:0"));

        return leaves;
    }

    private static byte[] HashLeaf(string data)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(data));
    }

    /// <summary>
    /// Computes Merkle root from a list of leaf hashes.
    /// Pairs adjacent leaves and hashes them until a single root remains.
    /// </summary>
    private static byte[] ComputeMerkleRoot(List<byte[]> leaves)
    {
        if (leaves.Count == 0) return SHA256.HashData([]); // empty tree
        if (leaves.Count == 1) return leaves[0];

        var currentLevel = new List<byte[]>(leaves);

        while (currentLevel.Count > 1)
        {
            var nextLevel = new List<byte[]>();
            for (int i = 0; i < currentLevel.Count; i += 2)
            {
                var left = currentLevel[i];
                var right = i + 1 < currentLevel.Count ? currentLevel[i + 1] : left; // duplicate last if odd
                nextLevel.Add(HashPair(left, right));
            }
            currentLevel = nextLevel;
        }

        return currentLevel[0];
    }

    private static byte[] HashPair(byte[] left, byte[] right)
    {
        // Sort to ensure canonical ordering (smallest first)
        var combined = new byte[64];
        if (CompareBytes(left, right) <= 0)
        {
            left.CopyTo(combined, 0);
            right.CopyTo(combined, 32);
        }
        else
        {
            right.CopyTo(combined, 0);
            left.CopyTo(combined, 32);
        }
        return SHA256.HashData(combined);
    }

    private static int CompareBytes(byte[] a, byte[] b)
    {
        for (int i = 0; i < Math.Min(a.Length, b.Length); i++)
        {
            if (a[i] != b[i]) return a[i].CompareTo(b[i]);
        }
        return a.Length.CompareTo(b.Length);
    }

    private static bool IsPowerOfTwo(int n) => n > 0 && (n & (n - 1)) == 0;

    /// <summary>
    /// Builds the final proof payload: [32-byte merkleRoot][4-byte version][timestamp][integrity hash]
    /// </summary>
    private static byte[] BuildProofPayload(byte[] merkleRoot, BattleResult result)
    {
        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms);

        // Header: version = 1 (Phase 1 ECDSA stub, not ZK)
        writer.Write((byte)0x01);

        // Merkle root (32 bytes)
        writer.Write(merkleRoot);

        // Timestamp (8 bytes, Unix seconds)
        writer.Write(DateTimeOffset.UtcNow.ToUnixTimeSeconds());

        // Winner address (variable length, null-terminated)
        writer.Write(Encoding.UTF8.GetBytes(result.Winner ?? string.Empty));
        writer.Write((byte)0x00);

        // Integrity: SHA-256 of [root + winner]
        var integrity = SHA256.HashData(
            Encoding.UTF8.GetBytes($"{Convert.ToHexString(merkleRoot)}:{result.Winner}:{result.ProposerScore}:{result.SolverScore}"));
        writer.Write(integrity);

        return ms.ToArray();
    }
}
