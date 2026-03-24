using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Events;
using AITrainingArena.Domain.ValueObjects;

namespace AITrainingArena.Domain.Entities;

/// <summary>
/// Represents a wATA token stake on an agent NFT.
/// Required amounts by class: A=100, B=500, C=2000, D=8000, E=30000.
/// </summary>
public class Stake
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>Unique identifier for this stake.</summary>
    public Guid StakeId { get; set; }

    /// <summary>NFT token ID of the staked agent.</summary>
    public uint NftId { get; set; }

    /// <summary>Wallet address of the staker.</summary>
    public WalletAddress StakerAddress { get; set; }

    /// <summary>Amount of wATA staked.</summary>
    public decimal Amount { get; set; }

    /// <summary>When the stake was created.</summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>When the stake lock expires (null if no lock).</summary>
    public DateTime? LockExpiresAt { get; set; }

    /// <summary>Whether the stake is currently locked.</summary>
    public bool IsLocked => LockExpiresAt.HasValue && LockExpiresAt.Value > DateTime.UtcNow;

    /// <summary>Domain events raised by this entity.</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Minimum wATA stake required per agent class.
    /// </summary>
    public static decimal GetMinimumStake(AgentClass agentClass) => agentClass switch
    {
        AgentClass.A => 100m,
        AgentClass.B => 500m,
        AgentClass.C => 2_000m,
        AgentClass.D => 8_000m,
        AgentClass.E => 30_000m,
        _ => throw new ArgumentOutOfRangeException(nameof(agentClass))
    };

    /// <summary>
    /// Creates a new stake and raises StakeCreated event.
    /// </summary>
    /// <param name="nftId">NFT to stake on.</param>
    /// <param name="stakerAddress">Staker wallet address.</param>
    /// <param name="amount">wATA amount to stake.</param>
    /// <param name="agentClass">Agent class for minimum validation.</param>
    /// <param name="lockDuration">Optional lock duration.</param>
    public static Stake Create(uint nftId, WalletAddress stakerAddress, decimal amount, AgentClass agentClass, TimeSpan? lockDuration = null)
    {
        var minimum = GetMinimumStake(agentClass);
        if (amount < minimum)
            throw new ArgumentException($"Minimum stake for class {agentClass} is {minimum} wATA, got {amount}.");

        var stake = new Stake
        {
            StakeId = Guid.NewGuid(),
            NftId = nftId,
            StakerAddress = stakerAddress,
            Amount = amount,
            CreatedAt = DateTime.UtcNow,
            LockExpiresAt = lockDuration.HasValue ? DateTime.UtcNow.Add(lockDuration.Value) : null
        };

        stake._domainEvents.Add(new StakeCreated(stake.StakeId, nftId, stakerAddress, amount, stake.CreatedAt));
        return stake;
    }

    /// <summary>
    /// Clears all pending domain events after they have been dispatched.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
