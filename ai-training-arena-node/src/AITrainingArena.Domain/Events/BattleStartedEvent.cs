namespace AITrainingArena.Domain.Events;

/// <summary>
/// Raised when a battle begins after handshake and role assignment.
/// </summary>
/// <param name="BattleId">Unique battle identifier.</param>
/// <param name="ProposerPeerId">Peer ID of the proposer.</param>
/// <param name="SolverPeerId">Peer ID of the solver.</param>
/// <param name="OccurredAt">When the battle started.</param>
public record BattleStartedEvent(
    Guid BattleId,
    string ProposerPeerId,
    string SolverPeerId,
    DateTime OccurredAt) : IDomainEvent;
