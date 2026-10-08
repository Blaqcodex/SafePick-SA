using FluentValidation;

namespace SafePick.Application.Transport.Learners.Commands.SetParentContact;

public sealed class SetParentContactValidator : AbstractValidator<SetParentContactCommand>
{
    public SetParentContactValidator()
    {
        RuleFor(x => x.LearnerId).NotEmpty();
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(32).When(x => x.Phone is not null);
        RuleFor(x => x.Name).MaximumLength(128).When(x => x.Name is not null);
        RuleFor(x => x.ConsentRecorded)
            .Equal(true)
            .WithMessage("POPIA consent must be recorded.");
    }
}