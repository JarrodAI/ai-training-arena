using AITrainingArena.Application.Commands;
using AITrainingArena.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AITrainingArena.API.Controllers;

[ApiController]
[Route("api/staking")]
public class StakingController : ControllerBase
{
    private readonly ISender _sender;
    private readonly NodeConfiguration _config;

    public StakingController(ISender sender, IOptions<NodeConfiguration> config)
    {
        _sender = sender;
        _config = config.Value;
    }

    [HttpPost("stake")]
    public async Task<IActionResult> Stake([FromBody] StakeTokensCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result ? Ok(new { staked = true }) : BadRequest(new { staked = false });
    }

    [HttpGet("rewards")]
    public async Task<IActionResult> GetPendingRewards(CancellationToken ct)
    {
        var rewards = await _sender.Send(new GetPendingRewardsQuery(_config.WalletAddress), ct);
        return Ok(new { pendingRewards = rewards });
    }

    [HttpPost("claim")]
    public async Task<IActionResult> ClaimRewards(CancellationToken ct)
    {
        var claimed = await _sender.Send(new ClaimRewardsCommand(_config.WalletAddress), ct);
        return Ok(new { claimedAmount = claimed });
    }
}
