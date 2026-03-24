using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.ValueObjects;

namespace AITrainingArena.Domain.Events;

/// <summary>
/// Raised when a new agent is registered on the platform.
/// </summary>
/// <param name="NftId">NFT token ID of the registered agent.</param>
/// <param name="AgentClass">Class tier of the agent.</param>
/// <param name="ModelName">AI model name.</param>
/// <param name="OwnerAddress">Owner wallet address.</param>
/// <param name="OccurredAt">When the agent was registered.</param>
public record AgentRegistered(
    uint NftId,
    AgentClass AgentClass,
    string ModelName,
    WalletAddress OwnerAddress,
    DateTime OccurredAt) : IDomainEvent;
