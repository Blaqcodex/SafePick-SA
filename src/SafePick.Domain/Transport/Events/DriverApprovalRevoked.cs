using SafePick.Domain.Common;

namespace SafePick.Domain.Transport.Events;

/// <summary>
/// Raised when a school revokes a previously granted driver approval.
/// The driver loses custody powers for that school.
/// </summary>
public sealed record DriverApprovalRevoked(
    Guid ApprovalId,
    Guid DriverId,
    Guid SchoolId,
    Guid DecidedByUserId,
    string Reason,
    DateTime OccurredOn) : IDomainEvent;