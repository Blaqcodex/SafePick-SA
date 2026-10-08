using FluentValidation;

namespace SafePick.Application.Custody.Commands.MarkSchoolAbsent;

public sealed class MarkSchoolAbsentValidator
    : AbstractValidator<MarkSchoolAbsentCommand>
{
    public MarkSchoolAbsentValidator()
    {
        RuleFor(x => x.SchoolId).NotEmpty();
        RuleFor(x => x.LearnerId).NotEmpty();
        RuleFor(x => x.ClientGeneratedId).NotEmpty();
        RuleFor(x => x.Note).MaximumLength(512);
    }
}