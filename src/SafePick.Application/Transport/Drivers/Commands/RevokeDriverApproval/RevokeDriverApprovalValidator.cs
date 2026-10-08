using FluentValidation;

namespace SafePick.Application.Transport.Drivers.Commands.RevokeDriverApproval;

public sealed class RevokeDriverApprovalValidator : AbstractValidator<RevokeDriverApprovalCommand>
{
    public RevokeDriverApprovalValidator()
    {
        RuleFor(x => x.ApprovalId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(512);
    }
}