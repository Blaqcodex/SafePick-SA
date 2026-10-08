namespace SafePick.Infrastructure.Pdf;

/// <summary>
/// Generates PDF receipts for custody events.
///
/// The interface is deliberately narrow: one method, one purpose.
/// A future system might add signature images, QR codes, or
/// cryptographic seals — but the current scope is a durable,
/// human-readable receipt that proves the pickup happened.
/// </summary>
public interface IPdfGenerator
{
    /// <summary>
    /// Generates a PDF receipt for a confirmed pickup.
    /// Returns the raw bytes of the generated file.
    /// </summary>
    byte[] GeneratePickupReceipt(PickupReceiptData data);
}

/// <summary>
/// Everything the receipt needs to display. Passed as a single
/// immutable record so the generator has no dependencies on
/// the database or on EF Core.
/// </summary>
public sealed record PickupReceiptData(
    Guid ReceiptId,
    string LearnerFullName,
    string LearnerClassGroup,
    string DriverFullName,
    string VehicleRegistrationNumber,
    string SchoolName,
    DateTime PickupTimestampUtc);