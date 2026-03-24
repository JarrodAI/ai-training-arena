using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Enums;

namespace AITrainingArena.Domain.Interfaces;

/// <summary>
/// Port for managing registered AI model agents.
/// </summary>
public interface IModelRegistry
{
    /// <summary>Retrieves an agent by its NFT token ID.</summary>
    /// <param name="nftId">The NFT token ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Agent?> GetAgentAsync(uint nftId, CancellationToken ct = default);

    /// <summary>Retrieves all agents of a specific class.</summary>
    /// <param name="agentClass">The agent class to filter by.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<Agent>> GetAgentsByClassAsync(AgentClass agentClass, CancellationToken ct = default);

    /// <summary>Registers a new agent in the model registry.</summary>
    /// <param name="agent">The agent to register.</param>
    /// <param name="ct">Cancellation token.</param>
    Task RegisterAgentAsync(Agent agent, CancellationToken ct = default);
}
