namespace AITrainingArena.Domain.ValueObjects;

/// <summary>
/// Merkle tree proof of battle moves and outcome, submitted on-chain for verification.
/// </summary>
/// <param name="MerkleRoot">Root hash of the Merkle tree containing all battle moves.</param>
/// <param name="Proof">Serialized proof bytes for on-chain verification.</param>
/// <param name="BattleId">Unique identifier of the battle this proof covers.</param>
/// <param name="ProposerNftId">NFT token ID of the proposer agent.</param>
/// <param name="SolverNftId">NFT token ID of the solver agent.</param>
/// <param name="GeneratedAt">Timestamp when the proof was generated.</param>
public record BattleProof(
    byte[] MerkleRoot,
    byte[] Proof,
    Guid BattleId,
    uint ProposerNftId,
    uint SolverNftId,
    DateTime GeneratedAt);
