using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafePick.Application.Common.Exceptions;
using SafePick.Application.Common.Interfaces;
using SafePick.Domain.Common;
using SafePick.Domain.Identity;
using SafePick.Domain.Transport;
using System.Security.Cryptography;
using System.Text;

namespace SafePick.Application.Identity.Commands.CreateInvitation;

public sealed partial class CreateInvitationHandler
    : IRequestHandler<CreateInvitationCommand, CreateInvitationResult>
{
    private static readonly TimeSpan InvitationValidity = TimeSpan.FromDays(7);
    private const string FrontendBaseUrl = "http://localhost:4200";

    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;
    private readonly IDateTime _clock;
    private readonly IInvitationMailer _mailer;
    private readonly ILogger<CreateInvitationHandler> _logger;

    public CreateInvitationHandler(
        IApplicationDbContext context,
        ICurrentUserService currentUser,
        IDateTime clock,
        IInvitationMailer mailer,
        ILogger<CreateInvitationHandler> logger)
    {
        _context = context;
        _currentUser = currentUser;
        _clock = clock;
        _mailer = mailer;
        _logger = logger;
    }

    public async Task<CreateInvitationResult> Handle(
        CreateInvitationCommand request,
        CancellationToken cancellationToken)
    {
        var now = _clock.UtcNow;

        var issuedByUserId = _currentUser.UserId
            ?? throw new ForbiddenAccessException("No authenticated user.");

        var issuer = await _context.Users
            .FirstOrDefaultAsync(u => u.Id == issuedByUserId, cancellationToken)
            ?? throw new ForbiddenAccessException("Issuer not found.");

        if (issuer.Role != Role.Operator)
            throw new ForbiddenAccessException(
                "Only the Operator may issue invitations.");

        var email = request.Email.Trim().ToLowerInvariant();

        var existingUser = await _context.Users
            .AnyAsync(u => u.Email == email, cancellationToken);

        if (existingUser)
            throw new DomainException("A user with this email already exists.");

        // 1. Create the School.
        var school = School.Register(
            request.SchoolName,
            request.SchoolContactEmail,
            now);

        _context.Schools.Add(school);

        // 2. Generate the raw token and hash it.
        var rawToken = GenerateRawToken();
        var tokenHash = HashToken(rawToken);

        // 3. Create the Invitation — storing ONLY the hash.
        var invitation = Invitation.Issue(
            email,
            Role.SchoolAdmin,
            school.Id,
            new InvitationToken(tokenHash),
            InvitationValidity,
            issuedByUserId,
            now);

        _context.Invitations.Add(invitation);

        // 4. Commit the invitation BEFORE attempting the email.
        await _context.SaveChangesAsync(cancellationToken);

        // 5. Build the invite URL.
        var inviteUrl =
            $"{FrontendBaseUrl}/accept-invitation?token={Uri.EscapeDataString(rawToken)}";

        // 6. Best-effort email delivery.
        var emailSent = await TrySendInvitationEmailAsync(
            invitation.Email,
            request.DisplayName,
            school.Name,
            inviteUrl,
            rawToken,
            invitation.ExpiresAt,
            cancellationToken);

        return new CreateInvitationResult(
            InvitationId: invitation.Id,
            SchoolId: school.Id,
            Email: invitation.Email,
            RawToken: rawToken,
            InviteUrl: inviteUrl,
            ExpiresAt: invitation.ExpiresAt,
            EmailSent: emailSent);
    }

    private async Task<bool> TrySendInvitationEmailAsync(
        string email,
        string displayName,
        string schoolName,
        string inviteUrl,
        string rawToken,
        DateTime expiresAt,
        CancellationToken cancellationToken)
    {
        try
        {
            await _mailer.SendInvitationAsync(
                recipientEmail: email,
                recipientName: displayName,
                schoolName: schoolName,
                inviteUrl: inviteUrl,
                rawToken: rawToken,
                expiresAt: expiresAt,
                cancellationToken: cancellationToken);

            return true;
        }
        catch (Exception ex)
        {
            LogEmailDispatchFailed(_logger, email, ex);
            return false;
        }
    }

    private static string GenerateRawToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(32);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');
    }

    private static string HashToken(string rawToken)
    {
        var bytes = Encoding.UTF8.GetBytes(rawToken);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Failed to dispatch invitation email for {Email}. " +
                  "The invitation exists; the Operator has the raw token.")]
    private static partial void LogEmailDispatchFailed(
        ILogger logger, string email, Exception exception);
}