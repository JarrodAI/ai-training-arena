namespace AITrainingArena.Domain.Events;

/// <summary>
/// Marker interface for domain events raised by aggregate roots.
/// </summary>
public interface IDomainEvent
{
    /// <summary>When the event occurred.</summary>
    DateTime OccurredAt { get; }
}
