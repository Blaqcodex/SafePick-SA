using MediatR;

namespace SafePick.Application.Transport.Assignments.Queries.GetAssignmentsBySchool;

/// <summary>
/// Returns the currently-open assignments for a school, enriched with
/// learner, driver, and vehicle names for direct rendering.
///
/// Used by the school admin's assignment management screen.
/// </summary>
public sealed record GetAssignmentsBySchoolQuery(
    Guid SchoolId,
    bool IncludeTerminated = false)
    : IRequest<IReadOnlyList<AssignmentItemDto>>;

public sealed record AssignmentItemDto(
    Guid AssignmentId,
    Guid SchoolId,
    Guid LearnerId,
    string LearnerFullName,
    string ClassGroup,
    Guid DriverId,
    string DriverFullName,
    Guid VehicleId,
    string VehicleRegistration,
    DateTime ValidFrom,
    DateTime? ValidTo);