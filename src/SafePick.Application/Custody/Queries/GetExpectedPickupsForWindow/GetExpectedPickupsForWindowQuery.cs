using MediatR;

namespace SafePick.Application.Custody.Queries.GetExpectedPickupsForWindow;

/// <summary>
/// Returns every custody expectation for a school on a given day:
/// derived from active assignments and the school's transport window.
///
/// This is *not* a query about what happened — it is a query about what
/// was *supposed* to happen. It is the input to the reconciliation engine,
/// which subtracts the custody events that resolved each expectation and
/// opens a gap for the remainder.
///
/// Expectations are computed, never stored (ADR-0009). Recomputing from
/// source truth means expectations cannot go stale when an assignment
/// changes or a learner is marked absent.
/// </summary>
public sealed record GetExpectedPickupsForWindowQuery(
    Guid SchoolId,
    DateOnly Date) : IRequest<IReadOnlyList<ExpectedPickupDto>>;

public sealed record ExpectedPickupDto(
    Guid SchoolId,
    Guid LearnerId,
    string LearnerFullName,
    string ClassGroup,
    Guid AssignmentId,
    Guid DriverId,
    string DriverName,
    Guid VehicleId,
    string VehicleRegistration,
    DateTime WindowStart,
    DateTime WindowEnd,
    DateTime ReconciliationAt,
    bool IsResolved,
    Guid? ResolvedByEventId,
    string? ResolutionType);