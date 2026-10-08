using SafePick.Domain.Common;

namespace SafePick.Domain.Identity.Events;

/// <summary>
/// Raised when a user's access is revoked. The user's authority is
/// withdrawn, but the record is preserved for audit.
/// </summary>
public sealed record UserDeactivated(
    Guid UserId,
    string Email,
    DateTime OccurredOn) : IDomainEvent;