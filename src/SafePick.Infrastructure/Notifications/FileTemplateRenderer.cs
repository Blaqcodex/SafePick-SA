using System.Text;
using Microsoft.Extensions.Logging;

namespace SafePick.Infrastructure.Notifications;

/// <summary>
/// Loads templates from the Templates folder and substitutes
/// {{Placeholder}} tokens with values from the provided data dictionary.
///
/// A minimal renderer, deliberately. Real template engines (Razor,
/// Scriban) are heavier. For SafePick SA's small set of templates,
/// simple token substitution is enough — and it's easy to audit.
/// </summary>
public sealed partial class FileTemplateRenderer : INotificationTemplateRenderer
{
    private readonly string _templatesDirectory;
    private readonly ILogger<FileTemplateRenderer> _logger;

    public FileTemplateRenderer(ILogger<FileTemplateRenderer> logger)
    {
        _logger = logger;
        _templatesDirectory = Path.Combine(
            AppContext.BaseDirectory, "Notifications", "Templates");
    }

    public async Task<RenderedTemplate> RenderAsync(
        string templateName,
        IReadOnlyDictionary<string, string> data,
        CancellationToken cancellationToken)
    {
        var htmlPath = Path.Combine(_templatesDirectory, $"{templateName}.html");
        var textPath = Path.Combine(_templatesDirectory, $"{templateName}.txt");
        var subjectPath = Path.Combine(_templatesDirectory, $"{templateName}.subject.txt");

        var htmlTemplate = await File.ReadAllTextAsync(htmlPath, cancellationToken);
        var textTemplate = await File.ReadAllTextAsync(textPath, cancellationToken);
        var subjectTemplate = await File.ReadAllTextAsync(subjectPath, cancellationToken);

        return new RenderedTemplate(
            Subject: Substitute(subjectTemplate, data).Trim(),
            HtmlBody: Substitute(htmlTemplate, data),
            PlainTextBody: Substitute(textTemplate, data));
    }

    private static string Substitute(
        string template,
        IReadOnlyDictionary<string, string> data)
    {
        var result = new StringBuilder(template);

        foreach (var (key, value) in data)
        {
            result.Replace($"{{{{{key}}}}}", value);
        }

        return result.ToString();
    }

    [LoggerMessage(
        Level = LogLevel.Information,
        Message = "Rendered template {TemplateName}")]
    static partial void LogRendered(ILogger logger, string templateName);
}