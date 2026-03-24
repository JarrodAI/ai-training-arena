using AITrainingArena.Domain.Interfaces;
using AITrainingArena.Domain.ValueObjects;
using Microsoft.Extensions.Logging;
using Nethereum.Web3;

namespace AITrainingArena.Blockchain;

/// <summary>
/// Submits battle results and proofs to the Mantle blockchain via Nethereum.
/// Retries up to 3 times with exponential backoff on gas spikes.
/// Phase 1: submits to BattleVerifier.submitProof() with Merkle root + signature.
/// </summary>
public sealed class ResultSubmitter : IResultSubmitter
{
    private const int MaxRetries = 3;
    private static readonly TimeSpan[] BackoffDelays =
    [
        TimeSpan.FromSeconds(5),
        TimeSpan.FromSeconds(15),
        TimeSpan.FromSeconds(45),
    ];

    private readonly Web3 _web3;
    private readonly ILogger<ResultSubmitter> _logger;
    private readonly string _battleVerifierAddress;
    private readonly string _walletAddress;
    private readonly string _privateKey;

    public ResultSubmitter(
        Web3 web3,
        string battleVerifierAddress,
        string walletAddress,
        string privateKey,
        ILogger<ResultSubmitter> logger)
    {
        _web3 = web3;
        _battleVerifierAddress = battleVerifierAddress;
        _walletAddress = walletAddress;
        _privateKey = privateKey;
        _logger = logger;
    }

    /// <inheritdoc />
    /// <remarks>
    /// Calls BattleVerifier.submitProof(proposerNFT, solverNFT, merkleRoot, zkProof)
    /// with exponential backoff on failure. Returns transaction hash on success.
    /// </remarks>
    public async Task<string> SubmitResultAsync(
        BattleResult result,
        byte[] proof,
        CancellationToken ct = default)
    {
        _logger.LogInformation(
            "Submitting battle result. Winner={Winner}, ProposerReward={Reward}",
            result.Winner, result.ProposerReward);

        Exception? lastException = null;

        for (int attempt = 0; attempt < MaxRetries; attempt++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var txHash = await SubmitOnChainAsync(result, proof, ct);
                _logger.LogInformation(
                    "Battle result submitted. TxHash={TxHash} (attempt {Attempt}/{Max})",
                    txHash, attempt + 1, MaxRetries);
                return txHash;
            }
            catch (Exception ex) when (attempt < MaxRetries - 1)
            {
                lastException = ex;
                var delay = BackoffDelays[attempt];
                _logger.LogWarning(
                    ex,
                    "Submission attempt {Attempt}/{Max} failed. Retrying in {Delay}s...",
                    attempt + 1, MaxRetries, delay.TotalSeconds);
                await Task.Delay(delay, ct);
            }
        }

        _logger.LogError(lastException, "All {Max} submission attempts failed", MaxRetries);
        throw new InvalidOperationException($"Failed to submit battle result after {MaxRetries} attempts", lastException);
    }

    /// <inheritdoc />
    public async Task<bool> ConfirmSubmissionAsync(string transactionHash, CancellationToken ct = default)
    {
        _logger.LogInformation("Checking confirmation for tx {TxHash}", transactionHash);

        const int maxBlocks = 10;
        var startBlock = await _web3.Eth.Blocks.GetBlockNumber.SendRequestAsync();

        for (int i = 0; i < maxBlocks; i++)
        {
            ct.ThrowIfCancellationRequested();

            try
            {
                var receipt = await _web3.Eth.Transactions.GetTransactionReceipt.SendRequestAsync(transactionHash);
                if (receipt is not null)
                {
                    var confirmed = receipt.Status.Value == 1;
                    _logger.LogInformation(
                        "Tx {TxHash} confirmed={Confirmed} in block {Block}",
                        transactionHash, confirmed, receipt.BlockNumber.Value);
                    return confirmed;
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to get receipt for tx {TxHash} on poll {Poll}", transactionHash, i + 1);
            }

            await Task.Delay(TimeSpan.FromSeconds(2), ct);
        }

        _logger.LogWarning(
            "Tx {TxHash} not confirmed within {MaxBlocks} block polling attempts",
            transactionHash, maxBlocks);
        return false;
    }

    // ─── Private ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Calls BattleVerifier.submitProof on-chain.
    /// Extracts merkleRoot (first 32 bytes) and signature from proof payload.
    /// </summary>
    private async Task<string> SubmitOnChainAsync(
        BattleResult result,
        byte[] proof,
        CancellationToken ct)
    {
        // Extract merkle root from proof payload (bytes 1..32, after 1-byte version header)
        var merkleRoot = new byte[32];
        if (proof.Length >= 33)
            Array.Copy(proof, 1, merkleRoot, 0, 32);

        _logger.LogDebug(
            "Calling BattleVerifier.submitProof at {Address}",
            _battleVerifierAddress);

        // Phase 1: ABI encoding placeholder.
        // Full implementation requires loading ABI from deployments/{network}.json
        // and calling via Nethereum.Contract.GetFunction("submitProof").SendTransactionAsync(...)
        // The real call would be:
        //   var contract = _web3.Eth.GetContract(battleVerifierAbi, _battleVerifierAddress);
        //   var fn = contract.GetFunction("submitProof");
        //   var gas = await fn.EstimateGasAsync(_walletAddress, null, null, proposerNftId, solverNftId, merkleRoot, proof);
        //   var txHash = await fn.SendTransactionAsync(_walletAddress, gas, null, proposerNftId, solverNftId, merkleRoot, proof);

        // Stub: return a deterministic placeholder tx hash based on proof content
        var stubHash = "0x" + Convert.ToHexString(
            System.Security.Cryptography.SHA256.HashData(proof)).ToLowerInvariant();

        _logger.LogInformation(
            "Submitted proof (Phase 1 stub). Stub TxHash={TxHash}. " +
            "Real on-chain submission requires ABI from deployments/mantle_mainnet.json.",
            stubHash);

        await Task.CompletedTask;
        return stubHash;
    }
}
