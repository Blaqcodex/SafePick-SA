using MediatR;

namespace SafePick.Application.Custody.Queries.GetOpenGapsBySchool;

/// <summary>
/// Returns the gap board for a school: every unresolved custody gap,
/// ordered by age (oldest first). This is the "work queue" for school
/// admins — every entry must eventually be resolved and closed.
/// </summary>
public sealed record GetOpenGapsBySchoolQuery(
    Guid SchoolId,
    bool IncludeResolved = false) : IRequest<IReadOnlyList<CustodyGapDto>>;

public sealed record CustodyGapDto(
    Guid GapId,
    Guid LearnerId,
    string LearnerFullName,
    string ClassGroup,
    Guid AssignmentId,
    Guid? DriverId,
    string? DriverName,
    string State,
    DateTime ExpectedWindowStart,
    DateTime ExpectedWindowEnd,
    DateTime OpenedAt,
    Guid? AcknowledgedByUserId,
    DateTime? AcknowledgedAt,
    DateTime? ResolvedAt,
    string? Resolution);