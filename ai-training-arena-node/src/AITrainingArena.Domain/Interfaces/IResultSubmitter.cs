using AITrainingArena.Domain.ValueObjects;

namespace AITrainingArena.Domain.Interfaces;

/// <summary>
/// Port for submitting battle results to the blockchain.
/// </summary>
public interface IResultSubmitter
{
    /// <summary>Submits a battle result and proof to the smart contract.</summary>
    /// <param name="result">The battle result.</param>
    /// <param name="proof">The serialized battle proof.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Transaction hash.</returns>
    Task<string> SubmitResultAsync(BattleResult result, byte[] proof, CancellationToken ct = default);

    /// <summary>Confirms a submission transaction was mined.</summary>
    /// <param name="transactionHash">The transaction hash to check.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> ConfirmSubmissionAsync(string transactionHash, CancellationToken ct = default);
}
