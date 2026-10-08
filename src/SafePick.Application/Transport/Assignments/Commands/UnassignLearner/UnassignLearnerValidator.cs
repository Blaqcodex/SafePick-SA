using FluentValidation;

namespace SafePick.Application.Transport.Assignments.Commands.UnassignLearner;

public sealed class UnassignLearnerValidator : AbstractValidator<UnassignLearnerCommand>
{
    public UnassignLearnerValidator()
    {
        RuleFor(x => x.AssignmentId).NotEmpty();
    }
}