using MediatR;

namespace SafePick.Application.Transport.Learners.Queries.GetLearnersBySchool;

/// <summary>
/// Returns the learners registered at a school, with their current
/// parent contact (if any) and their assignment status.
///
/// Used by the school admin's learner management screen.
/// </summary>
public sealed record GetLearnersBySchoolQuery(
    Guid SchoolId,
    bool IncludeInactive = false)
    : IRequest<IReadOnlyList<LearnerItemDto>>;

public sealed record LearnerItemDto(
    Guid LearnerId,
    Guid SchoolId,
    string FullName,
    GradeClassDto GradeClass,
    ParentContactDto? ParentContact,
    bool IsActive,
    Guid? CurrentAssignmentId,
    Guid? CurrentDriverId,
    string? CurrentDriverName,
    string? CurrentVehicleRegistration);

public sealed record GradeClassDto(
    int Grade,
    char Letter);

public sealed record ParentContactDto(
    string Email,
    string? Phone,
    string? Name,
    bool ConsentRecorded);