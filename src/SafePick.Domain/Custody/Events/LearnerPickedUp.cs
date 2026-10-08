using SafePick.Domain.Common;

namespace SafePick.Domain.Custody.Events;

/// <summary>
/// Raised when a learner is confirmed to have been picked up.
///
/// This is the primary custody event. It resolves the expectation for
/// that learner, on that school day, for that transport window.
///
/// The event is immutable and carries a client-generated idempotency key
/// so that duplicate submissions (e.g., from an offline driver syncing
/// twice) are deduplicated at the persistence layer.
/// </summary>
public sealed record LearnerPickedUp(
    Guid CustodyEventId,
    Guid SchoolId,
    Guid LearnerId,
    Guid DriverId,
    Guid VehicleId,
    Guid AssignmentId,
    Guid ClientGeneratedId,
    DateTime ServerTimestamp,
    DateTime? DeviceTimestamp,
    DateTime OccurredOn) : IDomainEvent;