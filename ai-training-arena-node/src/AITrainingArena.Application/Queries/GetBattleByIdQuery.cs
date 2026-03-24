using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Interfaces;
using MediatR;

namespace AITrainingArena.Application.Queries;

public record GetBattleByIdQuery(Guid BattleId) : IRequest<Battle?>;

public class GetBattleByIdQueryHandler : IRequestHandler<GetBattleByIdQuery, Battle?>
{
    private readonly IBattleRepository _battleRepository;

    public GetBattleByIdQueryHandler(IBattleRepository battleRepository)
        => _battleRepository = battleRepository;

    public async Task<Battle?> Handle(GetBattleByIdQuery request, CancellationToken cancellationToken)
        => await _battleRepository.GetByIdAsync(request.BattleId, cancellationToken);
}
