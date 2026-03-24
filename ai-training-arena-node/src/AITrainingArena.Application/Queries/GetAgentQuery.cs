using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Interfaces;
using MediatR;

namespace AITrainingArena.Application.Queries;

public record GetAgentQuery(uint NftId) : IRequest<Agent?>;

public class GetAgentQueryHandler : IRequestHandler<GetAgentQuery, Agent?>
{
    private readonly IAgentRepository _agentRepository;

    public GetAgentQueryHandler(IAgentRepository agentRepository)
        => _agentRepository = agentRepository;

    public async Task<Agent?> Handle(GetAgentQuery request, CancellationToken cancellationToken)
        => await _agentRepository.GetByNftIdAsync(request.NftId, cancellationToken);
}
