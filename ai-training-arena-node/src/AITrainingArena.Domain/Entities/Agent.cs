using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Events;
using AITrainingArena.Domain.ValueObjects;

namespace AITrainingArena.Domain.Entities;

/// <summary>
/// Core domain entity representing an AI agent registered on the platform.
/// Each agent is backed by an NFT and has an Elo rating per class.
/// </summary>
public class Agent
{
    private readonly List<IDomainEvent> _domainEvents = [];

    /// <summary>On-chain NFT token ID uniquely identifying this agent.</summary>
    public uint NftId { get; set; }

    /// <summary>Agent class tier (A-E) based on model parameter count.</summary>
    public AgentClass Class { get; set; }

    /// <summary>Name of the AI model powering this agent.</summary>
    public string ModelName { get; set; } = string.Empty;

    /// <summary>Current Elo rating (starts at 1500, per-class ranking).</summary>
    public EloRating EloRating { get; set; } = EloRating.Default();

    /// <summary>Total number of battles this agent has participated in.</summary>
    public int TotalBattles { get; set; }

    /// <summary>Total number of battles won.</summary>
    public int Wins { get; set; }

    /// <summary>Whether the agent is currently active and eligible for battles.</summary>
    public bool IsActive { get; set; }

    /// <summary>Amount of wATA staked on this agent.</summary>
    public decimal StakedAmount { get; set; }

    /// <summary>Ethereum/Mantle wallet address of the agent owner.</summary>
    public WalletAddress OwnerAddress { get; set; }

    /// <summary>Domain events raised by this entity.</summary>
    public IReadOnlyList<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Registers a new agent with default Elo and raises AgentRegistered event.
    /// </summary>
    /// <param name="nftId">NFT token ID.</param>
    /// <param name="agentClass">Agent class tier.</param>
    /// <param name="modelName">AI model name.</param>
    /// <param name="ownerAddress">Owner wallet address.</param>
    public static Agent Register(uint nftId, AgentClass agentClass, string modelName, WalletAddress ownerAddress)
    {
        var agent = new Agent
        {
            NftId = nftId,
            Class = agentClass,
            ModelName = modelName,
            EloRating = EloRating.Default(),
            TotalBattles = 0,
            Wins = 0,
            IsActive = true,
            StakedAmount = 0m,
            OwnerAddress = ownerAddress
        };

        agent._domainEvents.Add(new AgentRegistered(nftId, agentClass, modelName, ownerAddress, DateTime.UtcNow));
        return agent;
    }

    /// <summary>
    /// Records a battle result and updates Elo rating.
    /// </summary>
    /// <param name="opponentRating">Opponent's Elo rating.</param>
    /// <param name="won">Whether this agent won the battle.</param>
    public void RecordBattleResult(EloRating opponentRating, bool won)
    {
        TotalBattles++;
        if (won) Wins++;
        EloRating = EloRating.CalculateNew(opponentRating, won ? 1.0 : 0.0, TotalBattles);
    }

    /// <summary>
    /// Activates or deactivates the agent.
    /// </summary>
    /// <param name="active">New active state.</param>
    public void SetActive(bool active) => IsActive = active;

    /// <summary>
    /// Clears all pending domain events after they have been dispatched.
    /// </summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
