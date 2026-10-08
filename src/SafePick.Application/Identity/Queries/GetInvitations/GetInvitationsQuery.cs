using MediatR;

namespace SafePick.Application.Identity.Queries.GetInvitations;

/// <summary>
/// Lists the invitations issued by the Operator. Optionally filters
/// by status. Expired invitations are marked as such on read.
/// </summary>
public sealed record GetInvitationsQuery(
    string? Status = null)
    : IRequest<IReadOnlyList<InvitationItemDto>>;

public sealed record InvitationItemDto(
    Guid InvitationId,
    string Email,
    string Role,
    Guid SchoolId,
    string SchoolName,
    string Status,
    DateTime IssuedAt,
    DateTime ExpiresAt,
    DateTime? AcceptedAt);