using FluentValidation;

namespace SafePick.Application.Reconciliation.Commands.RunReconciliation;

public sealed class RunReconciliationValidator
    : AbstractValidator<RunReconciliationCommand>
{
    public RunReconciliationValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
    }
}