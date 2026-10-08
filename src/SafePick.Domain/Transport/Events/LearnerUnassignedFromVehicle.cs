using SafePick.Domain.Common;

namespace SafePick.Domain.Transport.Events;

/// <summary>
/// Raised when a learner's assignment to a vehicle is terminated.
/// The record is preserved (ValidTo is set), never deleted.
/// </summary>
public sealed record LearnerUnassignedFromVehicle(
    Guid AssignmentId,
    Guid SchoolId,
    Guid LearnerId,
    Guid VehicleId,
    DateTime TerminatedAt,
    Guid UnassignedByUserId,
    DateTime OccurredOn) : IDomainEvent;