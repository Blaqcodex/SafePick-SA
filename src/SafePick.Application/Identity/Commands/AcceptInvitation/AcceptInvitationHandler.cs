using MediatR;
using Microsoft.EntityFrameworkCore;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Common;
using SafePick.Domain.Identity;
using System.Security.Cryptography;
using System.Text;

namespace SafePick.Application.Identity.Commands.AcceptInvitation;

public sealed class AcceptInvitationHandler
    : IRequestHandler<AcceptInvitationCommand, AcceptInvitationResult>
{
    private readonly IApplicationDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IDateTime _clock;

    public AcceptInvitationHandler(
        IApplicationDbContext context,
        IPasswordHasher passwordHasher,
        IDateTime clock)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _clock = clock;
    }

    public async Task<AcceptInvitationResult> Handle(
        AcceptInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;
        var tokenHash = HashToken(request.RawToken);

        // Wrap the hash — same reason as in ValidateInvitationHandler.
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
            throw new DomainException("This invitation has expired.");
        }

        var email = invitation.Email.Trim().ToLowerInvariant();

        var emailTaken = await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (emailTaken)
        {
            throw new DomainException(
                "A user with this email already exists.");
        }

        var passwordHash = _passwordHasher.Hash(request.Password);

        var user = User.CreateFromInvitation(
            email: email,
            passwordHash: passwordHash,
            displayName: email.Split('@')[0],
            role: invitation.Role,
            schoolId: invitation.SchoolId);

        _context.Users.Add(user);

        invitation.Accept(now);

        await _context.SaveChangesAsync(cancellationToken);

        return new AcceptInvitationResult(
            UserId: user.Id,
            Email: user.Email,
            Role: user.Role.ToString(),
            SchoolId: invitation.SchoolId);
    }

    private static string HashToken(string rawToken)
    {
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}