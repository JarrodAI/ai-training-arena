using MediatR;

namespace AITrainingArena.Application.Commands;

public record StakeTokensCommand(uint NftId, decimal Amount) : IRequest<bool>;

public class StakeTokensCommandHandler : IRequestHandler<StakeTokensCommand, bool>
{
    public Task<bool> Handle(StakeTokensCommand request, CancellationToken cancellationToken)
    {
        // Placeholder — real implementation in Infrastructure layer
        return Task.FromResult(true);
    }
}
