using FluentValidation;

namespace SafePick.Application.Identity.Commands.SeedOperator;

/// <summary>
/// Validates the SeedOperator command before the handler runs.
/// This is input validation — the domain's invariants are separate.
/// </summary>
public sealed class SeedOperatorValidator : AbstractValidator<SeedOperatorCommand>
{
    public SeedOperatorValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(12)
            .MaximumLength(128);

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(128);
    }
}