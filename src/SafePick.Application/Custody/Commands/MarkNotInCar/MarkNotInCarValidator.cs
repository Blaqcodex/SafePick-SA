using FluentValidation;

namespace SafePick.Application.Custody.Commands.MarkNotInCar;

public sealed class MarkNotInCarValidator : AbstractValidator<MarkNotInCarCommand>
{
    public MarkNotInCarValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.LearnerId).NotEmpty();
        RuleFor(x => x.DriverId).NotEmpty();
        RuleFor(x => x.ClientGeneratedId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(512);
    }
}