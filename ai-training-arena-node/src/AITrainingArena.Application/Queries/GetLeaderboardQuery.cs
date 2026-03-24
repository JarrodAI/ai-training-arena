using AITrainingArena.Domain.Enums;
using AITrainingArena.Domain.Interfaces;
using AITrainingArena.Domain.ValueObjects;
using MediatR;

namespace AITrainingArena.Application.Queries;

public record GetLeaderboardQuery(AgentClass Class, int TopN = 100) : IRequest<List<LeaderboardEntry>>;

public class GetLeaderboardQueryHandler : IRequestHandler<GetLeaderboardQuery, List<LeaderboardEntry>>
{
    private readonly IAgentRepository _agentRepository;

    public GetLeaderboardQueryHandler(IAgentRepository agentRepository)
        => _agentRepository = agentRepository;

    public async Task<List<LeaderboardEntry>> Handle(
        GetLeaderboardQuery request, CancellationToken cancellationToken)
    {
        var agents = await _agentRepository.GetLeaderboardAsync(
            request.Class, request.TopN, cancellationToken);
        return agents.Select(a => new LeaderboardEntry(
            NftId: a.NftId,
            ModelName: a.ModelName,
            Class: a.Class,
            EloRating: a.EloRating,
            TotalBattles: a.TotalBattles,
            WinRate: a.TotalBattles > 0 ? (decimal)a.Wins / a.TotalBattles : 0m,
            TotalRewardsEarned: 0m
        )).ToList();
    }
}
