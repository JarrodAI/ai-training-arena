using AITrainingArena.Domain.ValueObjects;

namespace AITrainingArena.Domain.Interfaces;

/// <summary>
/// Port for generating and verifying Merkle tree battle proofs.
/// </summary>
public interface IProofGenerator
{
    /// <summary>Generates a Merkle proof for a completed battle.</summary>
    /// <param name="result">The battle result to generate proof for.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<byte[]> GenerateProofAsync(BattleResult result, CancellationToken ct = default);

    /// <summary>Verifies a battle proof against a result.</summary>
    /// <param name="proof">The proof bytes to verify.</param>
    /// <param name="result">The battle result the proof should match.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> VerifyProofAsync(byte[] proof, BattleResult result, CancellationToken ct = default);
}
