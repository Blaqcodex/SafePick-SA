using FluentValidation;

namespace SafePick.Application.Transport.Drivers.Commands.RejectDriver;

public sealed class RejectDriverValidator : AbstractValidator<RejectDriverCommand>
{
    public RejectDriverValidator()
    {
        RuleFor(x => x.ApprovalId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(512);
    }
}