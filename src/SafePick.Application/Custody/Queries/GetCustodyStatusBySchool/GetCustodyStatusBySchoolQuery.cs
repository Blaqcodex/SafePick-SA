using MediatR;

namespace SafePick.Application.Custody.Queries.GetCustodyStatusBySchool;

/// <summary>
/// Returns the school-wide custody monitoring board for a given day:
/// every expected learner with their current status, grouped by class.
/// </summary>
public sealed record GetCustodyStatusBySchoolQuery(
    Guid SchoolId,
    DateOnly Date) : IRequest<SchoolCustodyBoardDto>;

public sealed record SchoolCustodyBoardDto(
    Guid SchoolId,
    DateOnly Date,
    int TotalExpected,
    int PickedUp,
    int NotInCar,
    int MarkedAbsent,
    int Pending,
    int OpenGaps,
    IReadOnlyList<CustodyBoardRow> Rows);

public sealed record CustodyBoardRow(
    Guid LearnerId,
    string LearnerFullName,
    string ClassGroup,
    Guid? DriverId,
    string? DriverName,
    Guid? VehicleId,
    string? VehicleRegistration,
    string Status,
    Guid? CustodyEventId,
    DateTime? ResolvedAt);