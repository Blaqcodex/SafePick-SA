using FluentValidation;

namespace SafePick.Application.Reconciliation.Commands.AcknowledgeGap;

public sealed class AcknowledgeGapValidator
    : AbstractValidator<AcknowledgeGapCommand>
{
    public AcknowledgeGapValidator()
    {
        RuleFor(x => x.GapId).NotEmpty();
    }
}