using FluentValidation;

namespace SafePick.Application.Custody.Commands.ConfirmPickup;

public sealed class ConfirmPickupValidator : AbstractValidator<ConfirmPickupCommand>
{
    public ConfirmPickupValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.LearnerId).NotEmpty();
        RuleFor(x => x.DriverId).NotEmpty();
        RuleFor(x => x.ClientGeneratedId).NotEmpty();
    }
}