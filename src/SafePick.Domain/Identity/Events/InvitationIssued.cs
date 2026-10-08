using SafePick.Domain.Common;

namespace SafePick.Domain.Identity.Events;

/// <summary>
/// Raised when an actor with authority issues an invitation.
/// The handover of authority begins here.
/// </summary>
public sealed record InvitationIssued(
    Guid InvitationId,
    string Email,
    string Role,
    Guid SchoolId,
    Guid IssuedByUserId,
    DateTime OccurredOn) : IDomainEvent;