using SafePick.Domain.Common;

namespace SafePick.Domain.Transport.Events;

/// <summary>
/// Raised when a school rejects a driver's registration.
/// </summary>
public sealed record DriverRejected(
    Guid ApprovalId,
    Guid DriverId,
    Guid SchoolId,
    Guid DecidedByUserId,
    string Reason,
    DateTime OccurredOn) : IDomainEvent;