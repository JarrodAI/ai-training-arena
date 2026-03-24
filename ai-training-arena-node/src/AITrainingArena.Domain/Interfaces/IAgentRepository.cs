using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Enums;

namespace AITrainingArena.Domain.Interfaces;

/// <summary>
/// Repository port for persisting and querying agent entities.
/// </summary>
public interface IAgentRepository
{
    /// <summary>Retrieves an agent by its NFT token ID.</summary>
    /// <param name="nftId">The NFT token ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Agent?> GetByNftIdAsync(uint nftId, CancellationToken ct = default);

    /// <summary>Retrieves all agents owned by a wallet address.</summary>
    /// <param name="ownerAddress">The owner's wallet address.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<Agent>> GetByOwnerAsync(string ownerAddress, CancellationToken ct = default);

    /// <summary>Retrieves the leaderboard for a specific agent class.</summary>
    /// <param name="agentClass">The agent class to filter by.</param>
    /// <param name="topN">Number of top agents to return.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<Agent>> GetLeaderboardAsync(AgentClass agentClass, int topN = 100, CancellationToken ct = default);

    /// <summary>Persists a new or updated agent entity.</summary>
    /// <param name="agent">The agent to save.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SaveAsync(Agent agent, CancellationToken ct = default);
}
