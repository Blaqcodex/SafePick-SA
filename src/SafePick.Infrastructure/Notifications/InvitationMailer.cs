using SafePick.Application.Common.Interfaces;

namespace SafePick.Infrastructure.Notifications;

/// <summary>
/// Implementation of IInvitationMailer. Thin wrapper around
/// NotificationDispatcher, exposed as an interface so the
/// Application layer can call it without a direct reference.
/// </summary>
public sealed class InvitationMailer : IInvitationMailer
{
    private readonly NotificationDispatcher _dispatcher;

    public InvitationMailer(NotificationDispatcher dispatcher)
    {
        _dispatcher = dispatcher;
    }

    public Task SendInvitationAsync(
        string recipientEmail,
        string recipientName,
        string schoolName,
        string inviteUrl,
        string rawToken,
        DateTime expiresAt,
        CancellationToken cancellationToken)
        => _dispatcher.SendInvitationAsync(
            recipientEmail,
            recipientName,
            schoolName,
            inviteUrl,
            rawToken,
            expiresAt,
            cancellationToken);
}