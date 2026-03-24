using AITrainingArena.Application.Commands;
using AITrainingArena.Application.Queries;
using AITrainingArena.Domain.Enums;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AITrainingArena.API.Controllers;

/// <summary>
/// REST API for node status, leaderboard, and auto-battle control.
/// Implements all endpoints required by the Dioxus frontend at http://localhost:8081.
/// </summary>
[ApiController]
[Route("api")]
public class NodeController : ControllerBase
{
    private readonly ISender _sender;
    private readonly NodeConfiguration _config;

    public NodeController(ISender sender, IOptions<NodeConfiguration> config)
    {
        _sender = sender;
        _config = config.Value;
    }

    /// <summary>GET /api/status - node status: connected, agentClass, eloRating, autoBattle</summary>
    [HttpGet("status")]
    public async Task<IActionResult> GetStatus(CancellationToken ct)
    {
        var status = await _sender.Send(new GetNodeStatusQuery(), ct);
        return Ok(new
        {
            status.PeerId,
            connected = status.Status != NodeStatus.Offline,
            agentClass = _config.AgentClass.ToString(),
            eloRating = status.EloRating,
            autoBattle = _config.AutoBattle,
            connectedPeers = status.ConnectedPeers,
            completedBattles = status.CompletedBattles,
        });
    }

    /// <summary>GET /api/leaderboard/{class} - top 100 for agent class</summary>
    [HttpGet("leaderboard/{agentClass}")]
    public async Task<IActionResult> GetLeaderboard(AgentClass agentClass, CancellationToken ct)
    {
        var leaderboard = await _sender.Send(new GetLeaderboardQuery(agentClass), ct);
        return Ok(leaderboard);
    }

    /// <summary>GET /api/rewards - pending rewards</summary>
    [HttpGet("rewards")]
    public async Task<IActionResult> GetRewards(CancellationToken ct)
    {
        var rewards = await _sender.Send(new GetPendingRewardsQuery(_config.WalletAddress), ct);
        return Ok(new { pendingRewards = rewards });
    }

    /// <summary>POST /api/autobattle - toggle auto-battle on/off</summary>
    [HttpPost("autobattle")]
    public IActionResult ToggleAutoBattle([FromBody] ToggleAutoBattleRequest request)
    {
        _config.AutoBattle = request.Enabled;
        return Ok(new { autoBattle = _config.AutoBattle });
    }

    /// <summary>POST /api/claim - trigger reward claim transaction</summary>
    [HttpPost("claim")]
    public async Task<IActionResult> ClaimRewards(CancellationToken ct)
    {
        var claimed = await _sender.Send(new ClaimRewardsCommand(_config.WalletAddress), ct);
        return Ok(new { claimedAmount = claimed });
    }
}

public record ToggleAutoBattleRequest(bool Enabled);
