using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafePick.Domain.Custody;

namespace SafePick.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for the CustodyEvent aggregate.
///
/// Two important properties of this configuration:
///
///   1. A unique index on ClientGeneratedId — this is the database half
///      of the idempotency guarantee. If two concurrent requests slip
///      past the application-layer check, the second insert is rejected
///      by this index.
///
///   2. A composite index on (LearnerId, ServerTimestamp) — this is the
///      hot path for both the roster query and the history query.
///
/// The immutability of this table is enforced at the database level via
/// a REVOKE statement added in the migration (not here). See the
/// InitialCustody migration and ADR-0004.
/// </summary>
public sealed class CustodyEventConfiguration : IEntityTypeConfiguration<CustodyEvent>
{
    public void Configure(EntityTypeBuilder<CustodyEvent> builder)
    {
        builder.ToTable("custody_events");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id).HasColumnName("id");
        builder.Property(e => e.Type)
            .IsRequired()
            .HasConversion<int>()
            .HasColumnName("type");

        builder.Property(e => e.SchoolId).IsRequired().HasColumnName("school_id");
        builder.Property(e => e.LearnerId).IsRequired().HasColumnName("learner_id");

        builder.Property(e => e.DriverId).HasColumnName("driver_id");
        builder.Property(e => e.VehicleId).HasColumnName("vehicle_id");
        builder.Property(e => e.AssignmentId).HasColumnName("assignment_id");
        builder.Property(e => e.MarkedByUserId).HasColumnName("marked_by_user_id");

        builder.Property(e => e.ClientGeneratedId)
            .IsRequired()
            .HasColumnName("client_generated_id");

        // ServerTimestamp is a value object → mapped via conversion.
        builder.Property(e => e.ServerTimestamp)
            .IsRequired()
            .HasConversion(
                ts => ts.Value,
                value => new ServerTimestamp(value))
            .HasColumnName("server_timestamp");

        builder.Property(e => e.DeviceTimestamp).HasColumnName("device_timestamp");

        builder.Property(e => e.Reason).HasMaxLength(512).HasColumnName("reason");
        builder.Property(e => e.OriginalEventId).HasColumnName("original_event_id");
        builder.Property(e => e.Note).HasMaxLength(512).HasColumnName("note");

        // Idempotency: the same client-generated id can only be inserted once.
        builder.HasIndex(e => e.ClientGeneratedId)
            .IsUnique()
            .HasDatabaseName("ux_custody_events_client_generated_id");

        // Hot path for roster and history queries.
        builder.HasIndex(e => new { e.LearnerId, e.ServerTimestamp })
            .HasDatabaseName("ix_custody_events_learner_timestamp");

        // Index for the "events today for a school" query.
        builder.HasIndex(e => new { e.SchoolId, e.ServerTimestamp })
            .HasDatabaseName("ix_custody_events_school_timestamp");

        builder.Ignore(e => e.DomainEvents);
    }
}