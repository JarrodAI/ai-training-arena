using MediatR;

namespace AITrainingArena.Application.Queries;

public record GetPendingRewardsQuery(string WalletAddress) : IRequest<decimal>;

public class GetPendingRewardsQueryHandler : IRequestHandler<GetPendingRewardsQuery, decimal>
{
    public Task<decimal> Handle(GetPendingRewardsQuery request, CancellationToken cancellationToken)
    {
        // Placeholder — real implementation in Infrastructure layer
        return Task.FromResult(0m);
    }
}
