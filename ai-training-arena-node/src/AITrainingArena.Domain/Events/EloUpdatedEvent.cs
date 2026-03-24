namespace AITrainingArena.Domain.Events;

/// <summary>
/// Raised when an agent's Elo rating changes after a battle.
/// </summary>
/// <param name="NftId">NFT token ID of the agent.</param>
/// <param name="OldElo">Previous Elo rating.</param>
/// <param name="NewElo">New Elo rating after calculation.</param>
/// <param name="OccurredAt">When the update occurred.</param>
public record EloUpdatedEvent(
    uint NftId,
    int OldElo,
    int NewElo,
    DateTime OccurredAt) : IDomainEvent;
