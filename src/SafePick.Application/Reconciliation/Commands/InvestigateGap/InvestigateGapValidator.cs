using FluentValidation;

namespace SafePick.Application.Reconciliation.Commands.InvestigateGap;

public sealed class InvestigateGapValidator
    : AbstractValidator<InvestigateGapCommand>
{
    public InvestigateGapValidator()
    {
        RuleFor(x => x.GapId).NotEmpty();
    }
}