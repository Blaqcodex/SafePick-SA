namespace SafePick.Infrastructure.Notifications;

/// <summary>
/// A channel-agnostic notification message.
///
/// The dispatcher builds this. The channel translates it into its own
/// transport format — HTML email, SMS text, push payload, etc.
/// </summary>
public sealed class NotificationMessage
{
    public string To { get; init; } = string.Empty;
    public string? Cc { get; init; }
    public string Subject { get; init; } = string.Empty;
    public string HtmlBody { get; init; } = string.Empty;
    public string PlainTextBody { get; init; } = string.Empty;

    /// <summary>
    /// Optional in-memory attachment. The channel converts it to its
    /// native format. For email, this becomes a MIME attachment.
    /// </summary>
    public NotificationAttachment? Attachment { get; init; }
}

public sealed class NotificationAttachment
{
    public string FileName { get; init; } = string.Empty;
    public string ContentType { get; init; } = "application/pdf";
    public byte[] Content { get; init; } = Array.Empty<byte>();
}