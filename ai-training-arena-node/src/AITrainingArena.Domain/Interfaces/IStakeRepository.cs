using AITrainingArena.Domain.Entities;

namespace AITrainingArena.Domain.Interfaces;

/// <summary>
/// Repository port for persisting and querying stake entities.
/// </summary>
public interface IStakeRepository
{
    /// <summary>Retrieves a stake by its unique identifier.</summary>
    /// <param name="stakeId">The stake ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<Stake?> GetByIdAsync(Guid stakeId, CancellationToken ct = default);

    /// <summary>Retrieves all stakes for a specific agent NFT.</summary>
    /// <param name="nftId">The NFT token ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<IReadOnlyList<Stake>> GetByNftIdAsync(uint nftId, CancellationToken ct = default);

    /// <summary>Persists a new or updated stake entity.</summary>
    /// <param name="stake">The stake to save.</param>
    /// <param name="ct">Cancellation token.</param>
    Task SaveAsync(Stake stake, CancellationToken ct = default);
}
