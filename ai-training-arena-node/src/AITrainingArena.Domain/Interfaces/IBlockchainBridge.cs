using AITrainingArena.Domain.ValueObjects;

namespace AITrainingArena.Domain.Interfaces;

/// <summary>
/// Port for interacting with Mantle blockchain smart contracts.
/// Abstracts Nethereum calls behind a domain interface.
/// </summary>
public interface IBlockchainBridge
{
    /// <summary>Submits a battle proof to the BattleVerifier contract.</summary>
    /// <param name="proof">The battle proof to submit.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Transaction hash.</returns>
    Task<string> SubmitBattleProofAsync(BattleProof proof, CancellationToken ct = default);

    /// <summary>Claims ATA rewards for a completed battle.</summary>
    /// <param name="battleId">The battle ID to claim rewards for.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <returns>Transaction hash.</returns>
    Task<string> ClaimRewardsAsync(Guid battleId, CancellationToken ct = default);

    /// <summary>Reads the current on-chain Elo rating for an agent.</summary>
    /// <param name="nftId">The agent's NFT token ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<int> GetOnChainEloAsync(uint nftId, CancellationToken ct = default);

    /// <summary>Checks whether an agent NFT is currently active on-chain.</summary>
    /// <param name="nftId">The agent's NFT token ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<bool> IsAgentActiveAsync(uint nftId, CancellationToken ct = default);

    /// <summary>Reads the staked wATA balance for an agent.</summary>
    /// <param name="nftId">The agent's NFT token ID.</param>
    /// <param name="ct">Cancellation token.</param>
    Task<decimal> GetStakedBalanceAsync(uint nftId, CancellationToken ct = default);
}
