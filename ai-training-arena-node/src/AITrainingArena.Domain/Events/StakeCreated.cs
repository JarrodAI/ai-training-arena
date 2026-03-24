using AITrainingArena.Domain.ValueObjects;

namespace AITrainingArena.Domain.Events;

/// <summary>
/// Raised when a new wATA stake is created on an agent NFT.
/// </summary>
/// <param name="StakeId">Unique stake identifier.</param>
/// <param name="NftId">NFT token ID being staked on.</param>
/// <param name="StakerAddress">Wallet address of the staker.</param>
/// <param name="Amount">Amount of wATA staked.</param>
/// <param name="OccurredAt">When the stake was created.</param>
public record StakeCreated(
    Guid StakeId,
    uint NftId,
    WalletAddress StakerAddress,
    decimal Amount,
    DateTime OccurredAt) : IDomainEvent;
