using SafePick.Domain.Common;

namespace SafePick.Domain.Identity.Events;

/// <summary>
/// Raised when an invitation is redeemed and the user account is created.
/// The handover of authority completes here.
/// </summary>
public sealed record InvitationAccepted(
    Guid UserId,
    string Email,
    string Role,
    Guid SchoolId,
    DateTime OccurredOn) : IDomainEvent;