using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SafePick.Infrastructure.Notifications;

/// <summary>
/// The email implementation of INotificationChannel, using MailKit
/// against Gmail's SMTP server.
///
/// MailKit is preferred over System.Net.Mail because it has native
/// STARTTLS support, better error reporting, and is the recommended
/// library for modern SMTP in .NET.
/// </summary>
public sealed partial class EmailNotificationChannel : INotificationChannel
{
    private readonly NotificationSettings _settings;
    private readonly ILogger<EmailNotificationChannel> _logger;

    public EmailNotificationChannel(
        IOptions<NotificationSettings> settings,
        ILogger<EmailNotificationChannel> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public string ChannelName => "Email (SMTP)";

    public async Task SendAsync(
        NotificationMessage message,
        CancellationToken cancellationToken)
    {
        if (_settings.DryRun)
        {
            LogDryRun(_logger, message.To, message.Subject);
            return;
        }

        var mime = BuildMimeMessage(message);

        using var client = new SmtpClient();

        var socketOptions = _settings.UseStartTls
            ? SecureSocketOptions.StartTls
            : SecureSocketOptions.SslOnConnect;

        try
        {
            await client.ConnectAsync(
                _settings.SmtpHost,
                _settings.SmtpPort,
                socketOptions,
                cancellationToken);

            await client.AuthenticateAsync(
                _settings.SmtpUsername,
                _settings.SmtpPassword,
                cancellationToken);

            await client.SendAsync(mime, cancellationToken);

            await client.DisconnectAsync(true, cancellationToken);

            LogSent(_logger, message.To, message.Subject);
        }
        catch (Exception ex)
        {
            LogSendFailed(_logger, ex, message.To, message.Subject);
            throw;
        }
    }

    private MimeMessage BuildMimeMessage(NotificationMessage message)
    {
        var mime = new MimeMessage();

        mime.From.Add(new MailboxAddress(_settings.FromName, _settings.FromEmail));
        mime.To.Add(MailboxAddress.Parse(message.To));

        if (!string.IsNullOrWhiteSpace(message.Cc))
            mime.Cc.Add(MailboxAddress.Parse(message.Cc));

        mime.Subject = message.Subject;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = message.HtmlBody,
            TextBody = message.PlainTextBody
        };

        if (message.Attachment is not null)
        {
            bodyBuilder.Attachments.Add(
                message.Attachment.FileName,
                message.Attachment.Content,
                ContentType.Parse(message.Attachment.ContentType));
        }

        mime.Body = bodyBuilder.ToMessageBody();
        return mime;
    }

    // ---- Source-generated logging -----------------------------------------

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "[DRY-RUN] Would send email to {To} — subject: {Subject}")]
    static partial void LogDryRun(ILogger logger, string to, string subject);

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Email sent to {To} — subject: {Subject}")]
    static partial void LogSent(ILogger logger, string to, string subject);

    [LoggerMessage(
        Level = LogLevel.Error,
        Message = "Failed to send email to {To} — subject: {Subject}")]
    static partial void LogSendFailed(
        ILogger logger, Exception ex, string to, string subject);
}