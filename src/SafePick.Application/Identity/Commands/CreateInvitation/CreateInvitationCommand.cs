using MediatR;
using SafePick.Domain.Identity;

namespace SafePick.Application.Identity.Commands.CreateInvitation;

/// <summary>
/// Issues an invitation to a new School Admin, creating the School
/// in the process.
///
/// The Operator is the only role permitted to call this. The handler
/// creates BOTH the School and the Invitation — matching the domain's
/// design where the Operator provisions a school + its first admin
/// in a single action.
///
/// The handler returns the raw invitation token. It is never stored —
/// only its hash — so this is the caller's only chance to capture it.
/// </summary>
public sealed record CreateInvitationCommand(
    string Email,
    string DisplayName,
    string SchoolName,
    string SchoolContactEmail)
    : IRequest<CreateInvitationResult>;