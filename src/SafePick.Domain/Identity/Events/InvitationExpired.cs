using SafePick.Domain.Common;

namespace SafePick.Domain.Identity.Events;

/// <summary>
/// Raised when a pending invitation passes its expiry window without
/// being redeemed. The offered authority lapses.
/// </summary>
public sealed record InvitationExpired(
    Guid InvitationId,
    string Email,
    DateTime ExpiredAt,
    DateTime OccurredOn) : IDomainEvent;