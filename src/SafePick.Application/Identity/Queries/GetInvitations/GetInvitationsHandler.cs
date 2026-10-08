using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Identity;

namespace SafePick.Application.Identity.Queries.GetInvitations;

public sealed class GetInvitationsHandler
    : IRequestHandler<GetInvitationsQuery, IReadOnlyList<InvitationItemDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _clock;

    public GetInvitationsHandler(
        IApplicationDbContext context,
        IDateTime clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<IReadOnlyList<InvitationItemDto>> Handle(
        GetInvitationsQuery request,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;

        var query = _context.Invitations.AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Status) &&
            Enum.TryParse<InvitationStatus>(request.Status, true, out var parsedStatus))
        {
            query = query.Where(i => i.Status == parsedStatus);
        }

        var invitations = await query
            .OrderByDescending(i => i.IssuedAt)
            .ToListAsync(cancellationToken);

        if (invitations.Count == 0)
        {
            return Array.Empty<InvitationItemDto>();
        }

        var schoolIds = invitations.Select(i => i.SchoolId).Distinct().ToList();

        var schools = await _context.Schools
            .Where(s => schoolIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, s => s.Name, cancellationToken);

        return invitations.Select(i =>
        {
            // Compute effective status: a Pending invitation past its
            // expiry is treated as Expired for display purposes.
            var effectiveStatus = i.Status;
            if (effectiveStatus == InvitationStatus.Pending && now > i.ExpiresAt)
            {
                effectiveStatus = InvitationStatus.Expired;
            }

            return new InvitationItemDto(
                InvitationId: i.Id,
                Email: i.Email,
                Role: i.Role.ToString(),
                SchoolId: i.SchoolId,
                SchoolName: schools.TryGetValue(i.SchoolId, out var name)
                    ? name : "(unknown)",
                Status: effectiveStatus.ToString(),
                IssuedAt: i.IssuedAt,
                ExpiresAt: i.ExpiresAt,
                AcceptedAt: i.AcceptedAt);
        }).ToList();
    }
}