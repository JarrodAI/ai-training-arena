using AITrainingArena.Domain.Enums;

namespace AITrainingArena.Domain.Events;

/// <summary>
/// Raised when a P2P node's status changes (e.g., Available -> InBattle).
/// </summary>
/// <param name="PeerId">LibP2P peer identifier.</param>
/// <param name="OldStatus">Previous node status.</param>
/// <param name="NewStatus">New node status.</param>
/// <param name="OccurredAt">When the status change occurred.</param>
public record NodeStatusChangedEvent(
    string PeerId,
    NodeStatus OldStatus,
    NodeStatus NewStatus,
    DateTime OccurredAt) : IDomainEvent;
