using MediatR;

namespace SafePick.Application.Identity.Commands.AcceptInvitation;

/// <summary>
/// Accepts an invitation: validates the raw token, creates the
/// User with the provided password, and marks the invitation as
/// accepted.
///
/// The token is single-use. Once accepted, it cannot be used again.
/// </summary>
public sealed record AcceptInvitationCommand(
    string RawToken,
    string Password)
    : IRequest<AcceptInvitationResult>;

public sealed record AcceptInvitationResult(
    Guid UserId,
    string Email,
    string Role,
    Guid SchoolId);