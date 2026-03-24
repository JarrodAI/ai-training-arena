using AITrainingArena.Application.Commands;
using AITrainingArena.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AITrainingArena.API.Controllers;

/// <summary>
/// REST API for battle management: start, history, and individual battle lookup.
/// </summary>
[ApiController]
[Route("api/battles")]
public class BattleController : ControllerBase
{
    private readonly ISender _sender;
    private readonly NodeConfiguration _config;

    public BattleController(ISender sender, IOptions<NodeConfiguration> config)
    {
        _sender = sender;
        _config = config.Value;
    }

    /// <summary>POST /api/battles/start - find opponent and start battle</summary>
    [HttpPost("start")]
    public async Task<IActionResult> StartBattle(CancellationToken ct)
    {
        var command = new StartBattleCommand(_config.AgentClass, 1500);
        var result = await _sender.Send(command, ct);
        return result is null
            ? NotFound(new { error = "No opponent found" })
            : Ok(result);
    }

    /// <summary>GET /api/battles?last=20 - recent battle history</summary>
    [HttpGet]
    public async Task<IActionResult> GetBattleHistory(
        [FromQuery] int last = 20, CancellationToken ct = default)
    {
        var battles = await _sender.Send(new GetBattlesQuery(last), ct);
        return Ok(battles.Select(b => new
        {
            b.BattleId,
            b.ProposerId,
            b.SolverId,
            b.LocalRole,
            b.StartedAt,
            b.CompletedAt,
            b.Status,
            winner = b.FinalResult?.Winner,
            proposerScore = b.FinalResult?.ProposerScore,
            solverScore = b.FinalResult?.SolverScore,
        }));
    }

    /// <summary>GET /api/battles/{battleId} - single battle details</summary>
    [HttpGet("{battleId:guid}")]
    public async Task<IActionResult> GetBattle(Guid battleId, CancellationToken ct)
    {
        var battle = await _sender.Send(new GetBattleByIdQuery(battleId), ct);
        return battle is null ? NotFound(new { error = $"Battle {battleId} not found" }) : Ok(battle);
    }
}
