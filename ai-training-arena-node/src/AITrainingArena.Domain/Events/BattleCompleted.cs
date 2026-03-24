using AITrainingArena.Domain.ValueObjects;

namespace AITrainingArena.Domain.Events;

/// <summary>
/// Raised when a battle finishes and final scores are computed.
/// </summary>
/// <param name="BattleId">Unique battle identifier.</param>
/// <param name="ProposerId">Peer ID of the proposer agent.</param>
/// <param name="SolverId">Peer ID of the solver agent.</param>
/// <param name="Result">Final battle result with scores and rewards.</param>
/// <param name="OccurredAt">When the battle completed.</param>
public record BattleCompleted(
    Guid BattleId,
    string ProposerId,
    string SolverId,
    BattleResult Result,
    DateTime OccurredAt) : IDomainEvent;
