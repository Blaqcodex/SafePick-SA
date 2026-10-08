using FluentValidation;

namespace SafePick.Application.Transport.Learners.Commands.AddLearner;

public sealed class AddLearnerValidator : AbstractValidator<AddLearnerCommand>
{
    public AddLearnerValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(128);
        RuleFor(x => x.Grade).InclusiveBetween(1, 12);
        RuleFor(x => x.ClassLetter)
            .Must(c => char.IsLetter(c))
            .WithMessage("Class letter must be a single letter A–Z.");
    }
}