using FluentValidation;

namespace SafePick.Application.Transport.Assignments.Commands.ReassignLearner;

public sealed class ReassignLearnerValidator : AbstractValidator<ReassignLearnerCommand>
{
    public ReassignLearnerValidator()
    {
        RuleFor(x => x.LearnerId).NotEmpty();
        RuleFor(x => x.NewDriverId).NotEmpty();
    }
}