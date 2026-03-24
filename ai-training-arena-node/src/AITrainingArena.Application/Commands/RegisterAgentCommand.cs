using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Interfaces;
using AITrainingArena.Domain.ValueObjects;
using MediatR;

namespace AITrainingArena.Application.Commands;

public record RegisterAgentCommand(
    uint NftId,
    AgentClass Class,
    string ModelName,
    string OwnerAddress) : IRequest<bool>;

public class RegisterAgentCommandHandler : IRequestHandler<RegisterAgentCommand, bool>
{
    private readonly IAgentRepository _agentRepository;

    public RegisterAgentCommandHandler(IAgentRepository agentRepository)
        => _agentRepository = agentRepository;

    public async Task<bool> Handle(RegisterAgentCommand request, CancellationToken cancellationToken)
    {
        var owner = new WalletAddress(request.OwnerAddress);
        var agent = Agent.Register(request.NftId, request.Class, request.ModelName, owner);
        await _agentRepository.SaveAsync(agent, cancellationToken);
        return true;
    }
}
