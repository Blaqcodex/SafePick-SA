using MediatR;

namespace SafePick.Application.Identity.Queries.ValidateInvitation;

/// <summary>
/// Validates a raw invitation token without consuming it.
///
/// Used by the accept-invitation page to confirm the token is
/// still valid and to display the invitee's email and school.
///
/// The raw token is hashed server-side and matched against the
/// stored hash. This is the only place — other than accept —
/// where a raw token is presented.
/// </summary>
public sealed record ValidateInvitationQuery(string RawToken)
    : IRequest<ValidateInvitationResult>;

public sealed record ValidateInvitationResult(
    string Email,
    string Role,
    string SchoolName,
    DateTime ExpiresAt);