using FluentValidation;

namespace SafePick.Application.Custody.Commands.BatchConfirmPickups;

public sealed class BatchConfirmPickupsValidator
    : AbstractValidator<BatchConfirmPickupsCommand>
{
    public BatchConfirmPickupsValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.DriverId).NotEmpty();
        RuleFor(x => x.Items).NotEmpty();

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.LearnerId).NotEmpty();
            item.RuleFor(i => i.ClientGeneratedId).NotEmpty();
        });
    }
}