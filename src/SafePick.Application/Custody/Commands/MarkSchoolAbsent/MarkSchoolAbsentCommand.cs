using MediatR;

namespace SafePick.Application.Custody.Commands.MarkSchoolAbsent;

/// <summary>
/// Marks a learner as absent before the pickup window opens. This
/// suppresses the expectation for that day: the learner is not expected
/// to be collected, so no gap will open if no pickup is recorded.
/// </summary>
public sealed record MarkSchoolAbsentCommand(
    Guid SchoolId,
    Guid LearnerId,
    string? Note,
    Guid ClientGeneratedId) : IRequest<Guid>;