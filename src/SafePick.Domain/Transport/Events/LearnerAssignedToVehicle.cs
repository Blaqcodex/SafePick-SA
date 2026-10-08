using SafePick.Domain.Common;

namespace SafePick.Domain.Transport.Events;

/// <summary>
/// Raised when a learner is assigned to a vehicle/driver.
/// From the assignment's ValidFrom onward, the learner is expected
/// to be picked up by that driver.
/// </summary>
public sealed record LearnerAssignedToVehicle(
    Guid AssignmentId,
    Guid SchoolId,
    Guid LearnerId,
    Guid VehicleId,
    Guid DriverId,
    DateTime ValidFrom,
    Guid AssignedByUserId,
    DateTime OccurredOn) : IDomainEvent;