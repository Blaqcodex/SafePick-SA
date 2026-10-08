namespace SafePick.Infrastructure.Notifications;

/// <summary>
/// Abstraction over the transport that actually delivers a notification.
///
/// Only one implementation exists today — EmailNotificationChannel — but
/// the interface is designed for extensibility: a future
/// SmsNotificationChannel, WhatsAppNotificationChannel, or
/// PushNotificationChannel would implement the same contract, and the
/// dispatcher would not change.
///
/// The interface deliberately speaks in terms of "messages", not "emails".
/// The channel decides how to translate a message into its transport.
/// </summary>
public interface INotificationChannel
{
    /// <summary>
    /// Sends the given message. Throws on failure so the caller
    /// (typically the dispatcher) can record the outcome and decide
    /// whether to retry.
    /// </summary>
    Task SendAsync(NotificationMessage message, CancellationToken cancellationToken);

    /// <summary>
    /// Identifies this channel for logging and diagnostics.
    /// </summary>
    string ChannelName { get; }
}