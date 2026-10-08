using FluentValidation;

namespace SafePick.Application.Identity.Commands.CreateInvitation;

public sealed class CreateInvitationValidator
    : AbstractValidator<CreateInvitationCommand>
{
    public CreateInvitationValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.SchoolName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.SchoolContactEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);
    }
}