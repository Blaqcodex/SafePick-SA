using FluentValidation;

namespace SafePick.Application.Transport.Drivers.Commands.RegisterDriver;

public sealed class RegisterDriverValidator
    : AbstractValidator<RegisterDriverCommand>
{
    public RegisterDriverValidator()
    {
        RuleFor(x => x.FullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.IdNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.Phone)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(8)
            .WithMessage("Password must be at least 8 characters.");

        RuleFor(x => x.VehicleRegistrationNumber)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(x => x.VehicleMake)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.VehicleModel)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(x => x.VehicleCapacity)
            .GreaterThan(0)
            .LessThanOrEqualTo(100);

        RuleFor(x => x.SchoolId)
            .NotEmpty();
    }
}