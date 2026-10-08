using FluentValidation;

namespace SafePick.Application.Identity.Commands.AcceptInvitation;

public sealed class AcceptInvitationValidator
    : AbstractValidator<AcceptInvitationCommand>
{
    public AcceptInvitationValidator()
    {
        RuleFor(x => x.RawToken)
            .NotEmpty();

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.");
    }
}