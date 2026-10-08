using SafePick.Domain.Common;

namespace SafePick.Domain.Transport.Events;

/// <summary>
/// Raised when a school approves a driver. The driver now has custody
/// powers *for that school only*.
/// </summary>
public sealed record DriverApproved(
    Guid ApprovalId,
    Guid DriverId,
    Guid SchoolId,
    Guid DecidedByUserId,
    DateTime OccurredOn) : IDomainEvent;