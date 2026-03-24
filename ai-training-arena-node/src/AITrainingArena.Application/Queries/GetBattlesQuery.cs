using AITrainingArena.Domain.Entities;
using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Interfaces;
using MediatR;

namespace AITrainingArena.Application.Queries;

public record GetBattlesQuery(int LastN = 20) : IRequest<IReadOnlyList<Battle>>;

public class GetBattlesQueryHandler : IRequestHandler<GetBattlesQuery, IReadOnlyList<Battle>>
{
    private readonly IBattleRepository _battleRepository;

    public GetBattlesQueryHandler(IBattleRepository battleRepository)
        => _battleRepository = battleRepository;

    public async Task<IReadOnlyList<Battle>> Handle(
        GetBattlesQuery request, CancellationToken cancellationToken)
    {
        return await _battleRepository.GetByStatusAsync(
            BattleStatus.Completed, cancellationToken);
    }
}
