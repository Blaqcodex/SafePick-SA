using FluentValidation;

namespace SafePick.Application.Custody.Commands.RecordCorrection;

public sealed class RecordCorrectionValidator
    : AbstractValidator<RecordCorrectionCommand>
{
    public RecordCorrectionValidator()
    {
        RuleFor(x => x.OriginalEventId).NotEmpty();
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.LearnerId).NotEmpty();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1024);
        RuleFor(x => x.ClientGeneratedId).NotEmpty();
    }
}