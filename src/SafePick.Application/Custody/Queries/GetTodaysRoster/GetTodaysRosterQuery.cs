using MediatR;

namespace SafePick.Application.Custody.Queries.GetTodaysRoster;

/// <summary>
/// Returns the roster for a driver on a given date: every learner they are
/// expected to pick up, with the current custody status (pending, picked
/// up, not in car, school-marked-absent).
/// </summary>
public sealed record GetTodaysRosterQuery(
    Guid DriverId,
    DateOnly Date) : IRequest<IReadOnlyList<RosterItemDto>>;

public sealed record RosterItemDto(
    Guid LearnerId,
    string LearnerFullName,
    string ClassGroup,
    Guid AssignmentId,
    Guid SchoolId,
    Guid VehicleId,
    string VehicleRegistration,
    CustodyStatusDto Status,
    Guid? CustodyEventId,
    DateTime? ResolvedAt,
    string? Note);

public enum CustodyStatusDto
{
    Pending = 0,
    PickedUp = 1,
    NotInCar = 2,
    SchoolMarkedAbsent = 3,
    Corrected = 4
}