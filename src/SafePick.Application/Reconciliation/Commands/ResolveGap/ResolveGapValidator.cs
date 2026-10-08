using FluentValidation;

namespace SafePick.Application.Reconciliation.Commands.ResolveGap;

public sealed class ResolveGapValidator : AbstractValidator<ResolveGapCommand>
{
    public ResolveGapValidator()
    {
        RuleFor(x => x.GapId).NotEmpty();
        RuleFor(x => x.Resolution).NotEmpty().MaximumLength(1024);
    }
}