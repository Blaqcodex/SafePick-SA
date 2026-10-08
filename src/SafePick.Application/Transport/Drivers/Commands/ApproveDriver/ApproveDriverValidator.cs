using FluentValidation;

namespace SafePick.Application.Transport.Drivers.Commands.ApproveDriver;

public sealed class ApproveDriverValidator : AbstractValidator<ApproveDriverCommand>
{
    public ApproveDriverValidator()
    {
        RuleFor(x => x.ApprovalId).NotEmpty();
    }
}