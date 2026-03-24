using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Enums;

namespace AITrainingArena.Domain.Interfaces;

/// <summary>
/// Repository port for persisting and querying battle entities.
/// </summary>
public interface IBattleRepository
{
    /// <summary>Retrieves a battle by its unique identifier.</summary>
    /// <param name="battleId">The battle ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Battle?> GetByIdAsync(Guid battleId, CancellationToken ct = default);

    /// <summary>Retrieves battle history for a specific agent.</summary>
    /// <param name="agentPeerId">The agent's peer ID.</param>
    /// <param name="limit">Maximum number of results.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<Battle>> GetByAgentAsync(string agentPeerId, int limit = 50, CancellationToken ct = default);

    /// <summary>Retrieves battles filtered by status.</summary>
    /// <param name="status">The battle status to filter by.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<Battle>> GetByStatusAsync(BattleStatus status, CancellationToken ct = default);

    /// <summary>Persists a new or updated battle entity.</summary>
    /// <param name="battle">The battle to save.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SaveAsync(Battle battle, CancellationToken ct = default);
}
