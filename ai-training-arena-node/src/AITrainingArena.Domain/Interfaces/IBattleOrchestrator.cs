using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.ValueObjects;

namespace AITrainingArena.Domain.Interfaces;

/// <summary>
/// Port for the battle orchestration hosted service that manages the battle lifecycle.
/// </summary>
public interface IBattleOrchestrator
{
    /// <summary>Initiates a new battle between two agents.</summary>
    Task<Battle> InitiateBattleAsync(string proposerId, string solverId, CancellationToken ct = default);

    /// <summary>Runs a battle to completion.</summary>
    Task<BattleResult> RunBattleAsync(Guid battleId, CancellationToken ct = default);

    /// <summary>Retrieves an active or completed battle.</summary>
    Task<Battle?> GetBattleAsync(Guid battleId, CancellationToken ct = default);

    /// <summary>Executes the full battle loop: find opponent, connect, battle, submit proof.</summary>
    Task<BattleResult?> ExecuteBattleAsync(CancellationToken ct = default);

    /// <summary>Cancels an in-progress battle due to timeout or disconnect.</summary>
    Task CancelBattleAsync(Guid battleId, CancellationToken ct = default);
}
