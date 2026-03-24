using AITrainingArena.Application.Commands;
using FluentValidation;

namespace AITrainingArena.Application.Validators;

public class StakeTokensCommandValidator : AbstractValidator<StakeTokensCommand>
{
    public StakeTokensCommandValidator()
    {
        RuleFor(x => x.NftId)
            .GreaterThan((uint)0)
            .WithMessage("NftId must be greater than 0.");

        RuleFor(x => x.Amount)
            .GreaterThan(0m)
            .WithMessage("Amount must be greater than 0.");
    }
}
