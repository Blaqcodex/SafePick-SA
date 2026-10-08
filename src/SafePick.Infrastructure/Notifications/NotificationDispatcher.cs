using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SafePick.Domain.Custody;
using SafePick.Domain.Transport;
using SafePick.Infrastructure.Pdf;
using SafePick.Infrastructure.Persistence;

namespace SafePick.Infrastructure.Notifications;

/// <summary>
/// Orchestrates notification delivery.
///
/// The dispatcher receives domain information, resolves the actual
/// recipients, builds the message (with attachments if applicable),
/// and hands it to the notification channel.
///
/// Nothing in this class knows about SMTP. That's the channel's job.
///
/// TIME ZONE: any timestamp shown to a parent or school is converted
/// to South African Standard Time (SAST, UTC+2) at this boundary.
/// Underlying data remains UTC.
/// </summary>
public sealed partial class NotificationDispatcher
{
    private readonly INotificationChannel _channel;
    private readonly INotificationTemplateRenderer _templates;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly AppDbContext _context;
    private readonly ILogger<NotificationDispatcher> _logger;

    public NotificationDispatcher(
        INotificationChannel channel,
        INotificationTemplateRenderer templates,
        IPdfGenerator pdfGenerator,
        AppDbContext context,
        ILogger<NotificationDispatcher> logger)
    {
        _channel = channel;
        _templates = templates;
        _pdfGenerator = pdfGenerator;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Sends a pickup confirmation to the parent contact for the learner,
    /// with a PDF receipt attached.
    /// </summary>
    public async Task SendPickupConfirmationAsync(
        Guid schoolId,
        Guid learnerId,
        Guid custodyEventId,
        CancellationToken cancellationToken)
    {
        // Load learner + parent contact.
        var learner = await _context.Learners
            .FirstOrDefaultAsync(l => l.Id == learnerId, cancellationToken);

        if (learner?.ParentContact is null)
        {
            LogNoRecipient(_logger, learnerId, "PickupConfirmation");
            return;
        }

        // Load the custody event so we can get the driver, vehicle, timestamp.
        var custodyEvent = await _context.CustodyEvents
            .FirstOrDefaultAsync(e => e.Id == custodyEventId, cancellationToken);

        if (custodyEvent is null)
        {
            LogMissingEvent(_logger, custodyEventId);
            return;
        }

        // Load driver + vehicle.
        string driverName = "(unknown)";
        string vehicleRegistration = "(unknown)";

        if (custodyEvent.DriverId is not null)
        {
            var driver = await _context.Drivers
                .Include(d => d.Vehicle)
                .FirstOrDefaultAsync(d => d.Id == custodyEvent.DriverId, cancellationToken);

            if (driver is not null)
            {
                driverName = driver.FullName;
                vehicleRegistration = driver.Vehicle.RegistrationNumber;
            }
        }

        // Load school.
        var school = await _context.Schools
            .FirstOrDefaultAsync(s => s.Id == schoolId, cancellationToken);

        var schoolName = school?.Name ?? "(unknown school)";

        // Generate the PDF receipt.
        var receiptData = new PickupReceiptData(
            ReceiptId: custodyEvent.Id,
            LearnerFullName: learner.FullName,
            LearnerClassGroup: learner.ClassGroup.ToString(),
            DriverFullName: driverName,
            VehicleRegistrationNumber: vehicleRegistration,
            SchoolName: schoolName,
            PickupTimestampUtc: custodyEvent.ServerTimestamp.Value);

        byte[] pdfReceipt = _pdfGenerator.GeneratePickupReceipt(receiptData);

        // Convert the pickup time to SAST for display in the email body.
        var sastTime = ToSast(custodyEvent.ServerTimestamp.Value);

        // Render the email body.
        var rendered = await _templates.RenderAsync(
            "PickupConfirmation",
            new Dictionary<string, string>
            {
                ["LearnerName"] = learner.FullName,
                ["ClassGroup"] = learner.ClassGroup.ToString(),
                ["PickupTime"] = sastTime.ToString(
                    "yyyy-MM-dd HH:mm 'SAST'",
                    System.Globalization.CultureInfo.InvariantCulture)
            },
            cancellationToken);

        var message = new NotificationMessage
        {
            To = learner.ParentContact.Email,
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            PlainTextBody = rendered.PlainTextBody,
            Attachment = new NotificationAttachment
            {
                FileName = $"safepick-receipt-{custodyEvent.Id:N}.pdf",
                ContentType = "application/pdf",
                Content = pdfReceipt
            }
        };

        await _channel.SendAsync(message, cancellationToken);
    }

    /// <summary>
    /// Sends a "not in car" notification with the driver's reason.
    /// </summary>
    public async Task SendNotInCarAsync(
        Guid schoolId,
        Guid learnerId,
        string reason,
        CancellationToken cancellationToken)
    {
        var learner = await _context.Learners
            .FirstOrDefaultAsync(l => l.Id == learnerId, cancellationToken);

        if (learner?.ParentContact is null)
        {
            LogNoRecipient(_logger, learnerId, "NotInCar");
            return;
        }

        var rendered = await _templates.RenderAsync(
            "NotInCar",
            new Dictionary<string, string>
            {
                ["LearnerName"] = learner.FullName,
                ["ClassGroup"] = learner.ClassGroup.ToString(),
                ["Reason"] = reason
            },
            cancellationToken);

        var message = new NotificationMessage
        {
            To = learner.ParentContact.Email,
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            PlainTextBody = rendered.PlainTextBody
        };

        await _channel.SendAsync(message, cancellationToken);
    }

    /// <summary>
    /// Sends a custody gap alert to the parent and (if configured) the
    /// school's contact address.
    /// </summary>
    public async Task SendCustodyGapAlertAsync(
        Guid schoolId,
        Guid learnerId,
        CancellationToken cancellationToken)
    {
        var learner = await _context.Learners
            .FirstOrDefaultAsync(l => l.Id == learnerId, cancellationToken);

        var school = await _context.Schools
            .FirstOrDefaultAsync(s => s.Id == schoolId, cancellationToken);

        if (learner?.ParentContact is null)
        {
            LogNoRecipient(_logger, learnerId, "CustodyGapAlert");
            return;
        }

        var rendered = await _templates.RenderAsync(
            "CustodyGapAlert",
            new Dictionary<string, string>
            {
                ["LearnerName"] = learner.FullName,
                ["ClassGroup"] = learner.ClassGroup.ToString(),
                ["SchoolName"] = school?.Name ?? "(unknown school)"
            },
            cancellationToken);

        var message = new NotificationMessage
        {
            To = learner.ParentContact.Email,
            Cc = school?.ContactEmail,
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            PlainTextBody = rendered.PlainTextBody
        };

        await _channel.SendAsync(message, cancellationToken);
    }

    /// <summary>
    /// Sends an invitation email to a new School Admin.
    ///
    /// Called directly from the CreateInvitationHandler — NOT via the
    /// outbox. The outbox exists to guarantee side effects of immutable
    /// custody events. Invitations are neither immutable nor custody
    /// events, and the raw token must never be persisted. Sending
    /// directly keeps the token out of the database.
    ///
    /// NOTE: The expiry is converted to SAST for the recipient.
    /// </summary>
    public async Task SendInvitationAsync(
        string recipientEmail,
        string recipientName,
        string schoolName,
        string inviteUrl,
        string rawToken,
        DateTime expiresAt,
        CancellationToken cancellationToken)
    {
        var sastExpiresAt = ToSast(expiresAt);

        var rendered = await _templates.RenderAsync(
            "Invitation",
            new Dictionary<string, string>
            {
                ["RecipientName"] = recipientName,
                ["RecipientEmail"] = recipientEmail,
                ["SchoolName"] = schoolName,
                ["InviteUrl"] = inviteUrl,
                ["RawToken"] = rawToken,
                ["ExpiresAt"] = sastExpiresAt.ToString(
                    "dd MMM yyyy, HH:mm 'SAST'",
                    System.Globalization.CultureInfo.InvariantCulture)
            },
            cancellationToken);

        var message = new NotificationMessage
        {
            To = recipientEmail,
            Subject = rendered.Subject,
            HtmlBody = rendered.HtmlBody,
            PlainTextBody = rendered.PlainTextBody
        };

        await _channel.SendAsync(message, cancellationToken);
    }

    // ================================================================
    // SAST helpers — South African Standard Time (UTC+2, no DST).
    // ================================================================

    private static readonly TimeZoneInfo SastZone = ResolveSastZone();

    private static TimeZoneInfo ResolveSastZone()
    {
        var ids = new[]
        {
            "South Africa Standard Time",  // Windows
            "Africa/Johannesburg"          // Linux / macOS / IANA
        };

        foreach (var id in ids)
        {
            try { return TimeZoneInfo.FindSystemTimeZoneById(id); }
            catch (TimeZoneNotFoundException) { }
            catch (InvalidTimeZoneException) { }
        }

        return TimeZoneInfo.CreateCustomTimeZone(
            id: "SAST-Fixed",
            baseUtcOffset: TimeSpan.FromHours(2),
            displayName: "South Africa Standard Time",
            standardDisplayName: "SAST");
    }

    private static DateTime ToSast(DateTime utc)
    {
        var utcFixed = utc.Kind == DateTimeKind.Utc
            ? utc
            : DateTime.SpecifyKind(utc, DateTimeKind.Utc);

        return TimeZoneInfo.ConvertTimeFromUtc(utcFixed, SastZone);
    }

    // ================================================================
    // Logging
    // ================================================================

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "No parent contact for learner {LearnerId} — skipping {NotificationType} notification.")]
    static partial void LogNoRecipient(
        ILogger logger, Guid learnerId, string notificationType);

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Custody event {EventId} not found — skipping notification.")]
    static partial void LogMissingEvent(ILogger logger, Guid eventId);
}