using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafePick.Domain.Custody;

namespace SafePick.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for the CustodyGap aggregate.
///
/// Unlike custody events, gaps *are* mutable — they have a lifecycle.
/// A gap's state advances through Open → Acknowledged → Investigating →
/// Resolved → Closed, and each transition updates the row. Immutability
/// is not required here because the gap is a state, not a record of a
/// fact. The domain enforces the transitions.
/// </summary>
public sealed class CustodyGapConfiguration : IEntityTypeConfiguration<CustodyGap>
{
    public void Configure(EntityTypeBuilder<CustodyGap> builder)
    {
        builder.ToTable("custody_gaps");

        builder.HasKey(g => g.Id);

        builder.Property(g => g.Id).HasColumnName("id");
        builder.Property(g => g.SchoolId).IsRequired().HasColumnName("school_id");
        builder.Property(g => g.LearnerId).IsRequired().HasColumnName("learner_id");
        builder.Property(g => g.AssignmentId).IsRequired().HasColumnName("assignment_id");
        builder.Property(g => g.ReconciliationRunId)
            .IsRequired()
            .HasColumnName("reconciliation_run_id");

        builder.Property(g => g.ExpectedWindowStart)
            .IsRequired()
            .HasColumnName("expected_window_start");

        builder.Property(g => g.ExpectedWindowEnd)
            .IsRequired()
            .HasColumnName("expected_window_end");

        builder.Property(g => g.State)
            .IsRequired()
            .HasConversion<int>()
            .HasColumnName("state");

        builder.Property(g => g.AcknowledgedByUserId)
            .HasColumnName("acknowledged_by_user_id");

        builder.Property(g => g.AcknowledgedAt)
            .HasColumnName("acknowledged_at");

        builder.Property(g => g.ResolvedByUserId)
            .HasColumnName("resolved_by_user_id");

        builder.Property(g => g.ResolvedAt)
            .HasColumnName("resolved_at");

        builder.Property(g => g.ResolvedByCustodyEventId)
            .HasColumnName("resolved_by_custody_event_id");

        builder.Property(g => g.Resolution)
            .HasMaxLength(1024)
            .HasColumnName("resolution");

        builder.Property(g => g.OpenedAt)
            .IsRequired()
            .HasColumnName("opened_at");

        // The gap board query filters by (SchoolId, State).
        builder.HasIndex(g => new { g.SchoolId, g.State })
            .HasDatabaseName("ix_custody_gaps_school_state");

        // The "gaps for this learner" query.
        builder.HasIndex(g => g.LearnerId)
            .HasDatabaseName("ix_custody_gaps_learner");

        builder.Ignore(g => g.DomainEvents);
    }
}