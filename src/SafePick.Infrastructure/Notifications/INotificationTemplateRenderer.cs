namespace SafePick.Infrastructure.Notifications;

/// <summary>
/// Renders a notification template with the given data, returning the
/// subject, HTML body, and plain-text body.
/// </summary>
public interface INotificationTemplateRenderer
{
    Task<RenderedTemplate> RenderAsync(
        string templateName,
        IReadOnlyDictionary<string, string> data,
        CancellationToken cancellationToken);
}

public sealed record RenderedTemplate(
    string Subject,
    string HtmlBody,
    string PlainTextBody);