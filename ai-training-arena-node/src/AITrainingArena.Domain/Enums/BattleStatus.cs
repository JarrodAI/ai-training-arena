namespace AITrainingArena.Domain.Enums;

/// <summary>
/// State machine states for a battle lifecycle.
/// </summary>
public enum BattleStatus
{
    /// <summary>Battle created, waiting for both agents to connect.</summary>
    Pending,
    /// <summary>Handshake complete, roles assigned, battle in progress.</summary>
    InProgress,
    /// <summary>All rounds completed, computing final scores.</summary>
    Scoring,
    /// <summary>Proof generated and submitted to blockchain.</summary>
    ProofSubmitted,
    /// <summary>Battle verified on-chain, rewards distributed.</summary>
    Completed,
    /// <summary>Battle disputed, awaiting oracle resolution.</summary>
    Disputed,
    /// <summary>Battle cancelled due to timeout or disconnect.</summary>
    Cancelled
}
