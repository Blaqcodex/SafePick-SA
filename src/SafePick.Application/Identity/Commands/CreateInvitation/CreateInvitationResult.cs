namespace SafePick.Application.Identity.Commands.CreateInvitation;

/// <summary>
/// The result of issuing an invitation.
///
/// RawToken is the ONLY time the caller sees the token value — the
/// database stores only its hash. This is deliberately a transient
/// return value, not a persisted one.
///
/// EmailSent reports whether the invite email was delivered. When
/// false, the Operator still has the RawToken and InviteUrl and
/// can share them manually.
/// </summary>
public sealed record CreateInvitationResult(
    Guid InvitationId,
    Guid SchoolId,
    string Email,
    string RawToken,
    string InviteUrl,
    DateTime ExpiresAt,
    bool EmailSent);