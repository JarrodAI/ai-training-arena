using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Interfaces;
using AITrainingArena.Domain.ValueObjects;
using MediatR;

namespace AITrainingArena.Application.Commands;

public record StartBattleCommand(AgentClass MyClass, int MyElo) : IRequest<BattleResult?>;

public class StartBattleCommandHandler : IRequestHandler<StartBattleCommand, BattleResult?>
{
    private readonly IBattleOrchestrator _orchestrator;

    public StartBattleCommandHandler(IBattleOrchestrator orchestrator)
    {
        _orchestrator = orchestrator;
    }

    public async Task<BattleResult?> Handle(StartBattleCommand request, CancellationToken cancellationToken)
    {
        return await _orchestrator.ExecuteBattleAsync(cancellationToken);
    }
}
