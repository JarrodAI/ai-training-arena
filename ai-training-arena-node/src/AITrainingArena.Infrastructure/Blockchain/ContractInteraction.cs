using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace AITrainingArena.Infrastructure.Blockchain;

public class ContractInteraction
{
    private readonly MantleRpcClient _rpcClient;
    private readonly ILogger<ContractInteraction> _logger;

    public ContractInteraction(MantleRpcClient rpcClient, ILogger<ContractInteraction> logger)
    {
        _rpcClient = rpcClient;
        _logger = logger;
    }

    public async Task<Agent?> GetAgentInfoAsync(uint nftId)
    {
        _logger.LogInformation("Fetching on-chain agent info for NFT {NftId} (placeholder)", nftId);
        await Task.CompletedTask;
        return null;
    }

    public async Task<bool> AnnounceAvailabilityAsync(uint nftId, int eloRating, string peerId)
    {
        _logger.LogInformation(
            "Announcing availability for NFT {NftId}, ELO {Elo}, PeerId {PeerId} (placeholder)",
            nftId, eloRating, peerId);
        await Task.CompletedTask;
        return true;
    }

    public async Task<bool> UpdateNodeStatusAsync(uint nftId, NodeStatus status)
    {
        _logger.LogInformation(
            "Updating node status for NFT {NftId} to {Status} (placeholder)",
            nftId, status);
        await Task.CompletedTask;
        return true;
    }
}
