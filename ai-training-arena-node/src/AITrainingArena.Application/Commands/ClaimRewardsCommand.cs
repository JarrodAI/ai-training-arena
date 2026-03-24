using MediatR;

namespace AITrainingArena.Application.Commands;

public record ClaimRewardsCommand(string WalletAddress) : IRequest<decimal>;

public class ClaimRewardsCommandHandler : IRequestHandler<ClaimRewardsCommand, decimal>
{
    public Task<decimal> Handle(ClaimRewardsCommand request, CancellationToken cancellationToken)
    {
        // Placeholder — real implementation in Infrastructure layer
        return Task.FromResult(0m);
    }
}
