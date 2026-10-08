using SafePick.Domain.Common;

namespace SafePick.Domain.Custody.Events;

/// <summary>
/// Raised when a driver marks a learner as "not in the car" — the learner
/// was expected but was not collected. A reason is mandatory.
///
/// This resolves the expectation as *explained*. It is not a gap: the
/// driver has provided an accountable statement. The gap is reserved for
/// the case where *no statement at all* was made by window close.
/// </summary>
public sealed record LearnerNotInCar(
    Guid CustodyEventId,
    Guid SchoolId,
    Guid LearnerId,
    Guid DriverId,
    Guid VehicleId,
    Guid AssignmentId,
    Guid ClientGeneratedId,
    string Reason,
    DateTime ServerTimestamp,
    DateTime? DeviceTimestamp,
    DateTime OccurredOn) : IDomainEvent;