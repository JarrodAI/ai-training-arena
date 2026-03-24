using AITrainingArena.Application.Commands;
using AITrainingArena.Application.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace AITrainingArena.API.Controllers;

[ApiController]
[Route("api/agents")]
public class AgentController : ControllerBase
{
    private readonly ISender _sender;
    private readonly NodeConfiguration _config;

    public AgentController(ISender sender, IOptions<NodeConfiguration> config)
    {
        _sender = sender;
        _config = config.Value;
    }

    [HttpGet("{nftId:int}")]
    public async Task<IActionResult> GetAgent(uint nftId, CancellationToken ct)
    {
        var agent = await _sender.Send(new GetAgentQuery(nftId), ct);
        return agent is null ? NotFound() : Ok(agent);
    }

    [HttpGet("mine")]
    public async Task<IActionResult> GetMyAgent(CancellationToken ct)
    {
        var agent = await _sender.Send(new GetAgentQuery(_config.NftId), ct);
        return agent is null ? NotFound() : Ok(agent);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterAgentCommand command, CancellationToken ct)
    {
        var result = await _sender.Send(command, ct);
        return result ? Ok(new { registered = true }) : BadRequest(new { registered = false });
    }
}
