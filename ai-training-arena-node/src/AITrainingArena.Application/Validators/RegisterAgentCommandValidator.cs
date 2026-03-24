using AITrainingArena.Application.Commands;
using FluentValidation;

namespace AITrainingArena.Application.Validators;

public class RegisterAgentCommandValidator : AbstractValidator<RegisterAgentCommand>
{
    public RegisterAgentCommandValidator()
    {
        RuleFor(x => x.NftId)
            .GreaterThan((uint)0)
            .WithMessage("NftId must be greater than 0.");

        RuleFor(x => x.ModelName)
            .NotEmpty()
            .WithMessage("ModelName is required.")
            .MaximumLength(100)
            .WithMessage("ModelName must not exceed 100 characters.");

        RuleFor(x => x.OwnerAddress)
            .Must(addr => addr.StartsWith("0x") && addr.Length == 42)
            .WithMessage("OwnerAddress must start with '0x' and be 42 characters long.");
    }
}
