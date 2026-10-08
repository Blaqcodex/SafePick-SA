using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace SafePick.Infrastructure.Pdf;

/// <summary>
/// Generates pickup receipts using QuestPDF.
///
/// Design intent:
///   - A parent opens this in an email client or prints it. It must
///     communicate "your child was picked up at this time by this
///     driver" in under three seconds of glancing.
///   - Branded (SafePick SA navy + emerald), structured, calm.
///   - No decoration that competes with the information.
///
/// TIME ZONE: all displayed timestamps are shown in South African
/// Standard Time (SAST, UTC+2) — the operating time zone of every
/// school using this system. The underlying data remains UTC; the
/// conversion happens only at this display boundary.
///
/// Layout:
///   [emerald accent strip]
///   [brand header — name + subtitle + RECEIPT badge]
///   [confirmation block]
///   [learner block]
///   [transport block]
///   [receipt ID + immutability notice]
///   [footer]
/// </summary>
public sealed class QuestPdfReceiptGenerator : IPdfGenerator
{
    // ----------------------------------------------------------------
    // Brand palette — kept in sync with the design system.
    // ----------------------------------------------------------------
    private const string Navy       = "#1E3A5F";
    private const string NavyLight  = "#2C5282";
    private const string Emerald    = "#10B981";
    private const string EmeraldDk  = "#059669";
    private const string EmeraldBg  = "#ECFDF5";
    private const string Text       = "#111827";
    private const string TextMuted  = "#6B7280";
    private const string TextFaint  = "#9CA3AF";
    private const string Border     = "#E5E7EB";

    // ----------------------------------------------------------------
    // SAST — South African Standard Time (UTC+2, no DST).
    // ----------------------------------------------------------------
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

    // ----------------------------------------------------------------
    // Public entry point
    // ----------------------------------------------------------------
    public byte[] GeneratePickupReceipt(PickupReceiptData data)
    {
        QuestPDF.Settings.License = LicenseType.Community;

        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A5);
                page.Margin(0);
                page.PageColor(Colors.White);
                page.DefaultTextStyle(x => x.FontSize(10).FontFamily("Helvetica"));

                // ---------------------------------------------
                // Header
                // ---------------------------------------------
                page.Header().Column(header =>
                {
                    header.Item().Background(Emerald).Height(6);

                    header.Item()
                        .PaddingHorizontal(1.5f, Unit.Centimetre)
                        .PaddingTop(18)
                        .PaddingBottom(14)
                        .Row(row =>
                        {
                            row.RelativeItem().Column(c =>
                            {
                                c.Item().Text("SafePick SA")
                                    .FontSize(20).Bold().FontColor(Navy);
                                c.Item().PaddingTop(2)
                                    .Text("Digital Learner Transport Verification")
                                    .FontSize(8).FontColor(TextMuted);
                            });

                            row.ConstantItem(110).AlignRight().Column(c =>
                            {
                                c.Item()
                                    .Background(Navy)
                                    .PaddingVertical(4).PaddingHorizontal(10)
                                    .Text("PICKUP RECEIPT")
                                    .FontSize(8).Bold().FontColor(Colors.White)
                                    .LetterSpacing(0.08f);
                            });
                        });

                    header.Item()
                        .PaddingHorizontal(1.5f, Unit.Centimetre)
                        .LineHorizontal(1).LineColor(Border);
                });

                // ---------------------------------------------
                // Content
                // ---------------------------------------------
                page.Content()
                    .PaddingHorizontal(1.5f, Unit.Centimetre)
                    .PaddingVertical(20)
                    .Column(column =>
                    {
                        column.Spacing(18);

                        // ---- Confirmation badge ----
                        column.Item()
                            .Background(EmeraldBg)
                            .BorderLeft(3).BorderColor(Emerald)
                            .Padding(12)
                            .Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("CUSTODY CONFIRMED")
                                        .FontSize(10).Bold().FontColor(EmeraldDk)
                                        .LetterSpacing(0.06f);
                                    c.Item().PaddingTop(2)
                                        .Text("This learner was picked up as scheduled.")
                                        .FontSize(9).FontColor(TextMuted);
                                });
                            });

                        // ---- Section: Learner ----
                        column.Item().Column(section =>
                        {
                            SectionHeader(section, "Learner");
                            section.Item().PaddingTop(8).Column(body =>
                            {
                                body.Spacing(8);
                                DetailRow(body, "Name",  data.LearnerFullName);
                                DetailRow(body, "Class", data.LearnerClassGroup);
                                DetailRow(body, "School", data.SchoolName);
                            });
                        });

                        // ---- Section: Transport ----
                        column.Item().Column(section =>
                        {
                            SectionHeader(section, "Transport");
                            section.Item().PaddingTop(8).Column(body =>
                            {
                                body.Spacing(8);
                                DetailRow(body, "Driver",  data.DriverFullName);
                                DetailRow(body, "Vehicle", data.VehicleRegistrationNumber);
                                DetailRow(body, "Pickup time (SAST)",
                                    ToSast(data.PickupTimestampUtc).ToString(
                                        "yyyy-MM-dd HH:mm:ss 'SAST'",
                                        System.Globalization.CultureInfo.InvariantCulture));
                            });
                        });

                        // ---- Receipt ID block ----
                        column.Item()
                            .PaddingTop(4)
                            .BorderTop(1).BorderColor(Border)
                            .PaddingTop(12)
                            .Column(idBlock =>
                            {
                                idBlock.Item().Text("RECEIPT ID")
                                    .FontSize(8).FontColor(TextFaint)
                                    .LetterSpacing(0.08f);
                                idBlock.Item().PaddingTop(2)
                                    .Text(data.ReceiptId.ToString())
                                    .FontSize(9).FontFamily("Courier New")
                                    .FontColor(Text);
                            });

                        // ---- Immutability notice ----
                        column.Item().PaddingTop(4)
                            .Text(
                                "This receipt is a durable record of custody. " +
                                "It cannot be modified or deleted. Any correction " +
                                "is recorded as a separate, traceable event.")
                            .FontSize(8).Italic().FontColor(TextMuted)
                            .LineHeight(1.4f);
                    });

                // ---------------------------------------------
                // Footer
                // ---------------------------------------------
                page.Footer()
                    .PaddingHorizontal(1.5f, Unit.Centimetre)
                    .PaddingBottom(14)
                    .BorderTop(1).BorderColor(Border)
                    .PaddingTop(8)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Text("SafePick SA — Digital Learner Transport Verification")
                            .FontSize(7).FontColor(TextFaint);

                        row.ConstantItem(190).AlignRight()
                            .Text(text =>
                            {
                                text.Span("Generated ").FontSize(7).FontColor(TextFaint);
                                text.Span(ToSast(DateTime.UtcNow).ToString(
                                    "yyyy-MM-dd HH:mm 'SAST'",
                                    System.Globalization.CultureInfo.InvariantCulture))
                                    .FontSize(7).FontColor(TextFaint);
                            });
                    });
            });
        });

        return document.GeneratePdf();
    }

    // ================================================================
    // Helpers
    // ================================================================

    private static void SectionHeader(
        QuestPDF.Fluent.ColumnDescriptor column,
        string title)
    {
        column.Item().Row(row =>
        {
            row.AutoItem()
                .Text(title.ToUpperInvariant())
                .FontSize(8).Bold().FontColor(NavyLight)
                .LetterSpacing(0.10f);

            row.RelativeItem()
                .PaddingLeft(8).AlignMiddle()
                .LineHorizontal(1).LineColor(Border);
        });
    }

    private static void DetailRow(
        QuestPDF.Fluent.ColumnDescriptor column,
        string label,
        string value)
    {
        column.Item().Row(row =>
        {
            row.ConstantItem(110)
                .Text(label)
                .FontSize(9).FontColor(TextMuted);

            row.RelativeItem()
                .Text(value)
                .FontSize(11).SemiBold().FontColor(Text);
        });
    }
}