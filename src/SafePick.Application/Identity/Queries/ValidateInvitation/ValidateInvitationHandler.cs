using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Common;
using SafePick.Domain.Identity;
using System.Security.Cryptography;
using System.Text;

namespace SafePick.Application.Identity.Queries.ValidateInvitation;

public sealed class ValidateInvitationHandler
    : IRequestHandler<ValidateInvitationQuery, ValidateInvitationResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IDateTime _clock;

    public ValidateInvitationHandler(
        IApplicationDbContext context,
        IDateTime clock)
    {
        _context = context;
        _clock = clock;
    }

    public async Task<ValidateInvitationResult> Handle(
        ValidateInvitationQuery request,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var tokenHash = HashToken(request.RawToken);

        // Wrap the hash in the value object so EF Core's converter
        // translates the comparison to SQL. Do NOT reach through .Value
        // inside the LINQ expression — it won't translate.
        var token = new InvitationToken(tokenHash);

        var invitation = await _context.Invitations
            .FirstOrDefaultAsync(i => i.Token == token, cancellationToken)
            ?? throw new NotFoundException("Invitation", request.RawToken);

        if (invitation.Status != InvitationStatus.Pending)
        {
            throw new DomainException(
                $"This invitation is no longer valid (status: {invitation.Status}).");
        }

        if (now > invitation.ExpiresAt)
        {
            throw new DomainException(
                "This invitation has expired. Please ask the Operator for a new one.");
        }

        var school = await _context.Schools
            .FirstOrDefaultAsync(s => s.Id == invitation.SchoolId, cancellationToken);

        return new ValidateInvitationResult(
            Email: invitation.Email,
            Role: invitation.Role.ToString(),
            SchoolName: school?.Name ?? "(unknown)",
            ExpiresAt: invitation.ExpiresAt);
    }

    private static string HashToken(string rawToken)
    {
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}