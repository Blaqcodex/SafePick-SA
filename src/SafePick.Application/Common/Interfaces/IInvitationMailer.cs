namespace SafePick.Application.Common.Interfaces;

/// <summary>
/// Abstraction for sending an invitation email.
///
/// This interface lives in the Application layer so that the
/// CreateInvitationHandler can send invitation emails WITHOUT
/// depending on the Infrastructure layer directly.
///
/// The implementation lives in Infrastructure and wraps the
/// NotificationDispatcher. This preserves the Clean Architecture
/// dependency direction: Application defines the contract,
/// Infrastructure provides the implementation.
/// </summary>
public interface IInvitationMailer
{
    /// <summary>
    /// Sends an invitation email. Called directly (not via the outbox)
    /// because the raw token must never be persisted.
    ///
    /// Implementations are expected to throw on delivery failure;
    /// the caller handles failure.
    /// </summary>
    Task SendInvitationAsync(
        string recipientEmail,
        string recipientName,
        string schoolName,
        string inviteUrl,
        string rawToken,
        DateTime expiresAt,
        CancellationToken cancellationToken);
}