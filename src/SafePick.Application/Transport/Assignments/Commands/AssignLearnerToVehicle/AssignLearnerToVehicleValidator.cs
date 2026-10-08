using FluentValidation;

namespace SafePick.Application.Transport.Assignments.Commands.AssignLearnerToVehicle;

public sealed class AssignLearnerToVehicleValidator
    : AbstractValidator<AssignLearnerToVehicleCommand>
{
    public AssignLearnerToVehicleValidator()
    {
        RuleFor(x => x.LearnerId).NotEmpty();
        RuleFor(x => x.DriverId).NotEmpty();
    }
}