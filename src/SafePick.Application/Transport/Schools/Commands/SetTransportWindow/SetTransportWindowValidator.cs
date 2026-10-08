using FluentValidation;

namespace SafePick.Application.Transport.Schools.Commands.SetTransportWindow;

public sealed class SetTransportWindowValidator
    : AbstractValidator<SetTransportWindowCommand>
{
    public SetTransportWindowValidator()
    {
        RuleFor(x => x.SchoolId)
            .NotEmpty();

        RuleFor(x => x.ClosesAt)
            .GreaterThan(x => x.OpensAt)
            .WithMessage("Close time must be after open time.");

        RuleFor(x => x.GraceMinutes)
            .GreaterThanOrEqualTo(0)
            .LessThanOrEqualTo(240)
            .WithMessage("Grace period must be between 0 and 240 minutes.");
    }
}