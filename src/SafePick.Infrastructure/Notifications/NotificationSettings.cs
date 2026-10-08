namespace SafePick.Infrastructure.Notifications;

/// <summary>
/// Configuration for the email notification channel. Bound from the
/// "Notifications" section of appsettings.
///
/// The credentials in this section are supplied via
/// appsettings.Development.json (gitignored), never appsettings.json.
/// </summary>
public sealed class NotificationSettings
{
    /// <summary>
    /// The "from" address that appears in outgoing emails. For Gmail,
    /// this must match the authenticated account.
    /// </summary>
    public string FromEmail { get; set; } = string.Empty;

    /// <summary>
    /// The display name for the "from" address, e.g. "SafePick SA".
    /// </summary>
    public string FromName { get; set; } = "SafePick SA";

    /// <summary>
    /// The SMTP host. For Gmail: "smtp.gmail.com".
    /// </summary>
    public string SmtpHost { get; set; } = "smtp.gmail.com";

    /// <summary>
    /// The SMTP port. For Gmail STARTTLS: 587. For SSL: 465.
    /// </summary>
    public int SmtpPort { get; set; } = 587;

    /// <summary>
    /// Whether to use STARTTLS (true for port 587) or implicit SSL
    /// (true for port 465).
    /// </summary>
    public bool UseStartTls { get; set; } = true;

    /// <summary>
    /// The Gmail account used for authentication.
    /// </summary>
    public string SmtpUsername { get; set; } = string.Empty;

    /// <summary>
    /// The Gmail App Password. Never committed to source control.
    /// </summary>
    public string SmtpPassword { get; set; } = string.Empty;

    /// <summary>
    /// When true, emails are logged to the console but not actually sent.
    /// </summary>
    public bool DryRun { get; set; }
}