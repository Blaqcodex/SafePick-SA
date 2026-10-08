using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafePick.Domain.Custody;

namespace SafePick.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for the ReconciliationRun aggregate.
///
/// The run is a mutable record of a single execution of the reconciliation
/// engine. Its outcomes are stored as an owned collection — each outcome
/// is a value object, materialized from its own row but not independently
/// addressable.
/// </summary>
public sealed class ReconciliationRunConfiguration
    : IEntityTypeConfiguration<ReconciliationRun>
{
    public void Configure(EntityTypeBuilder<ReconciliationRun> builder)
    {
        builder.ToTable("reconciliation_runs");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id).HasColumnName("id");
        builder.Property(r => r.SchoolId).IsRequired().HasColumnName("school_id");
        builder.Property(r => r.SchoolDay).IsRequired().HasColumnName("school_day");
        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<int>()
            .HasColumnName("status");
        builder.Property(r => r.StartedAt).IsRequired().HasColumnName("started_at");
        builder.Property(r => r.CompletedAt).HasColumnName("completed_at");
        builder.Property(r => r.ExpectationsEvaluated)
            .IsRequired()
            .HasColumnName("expectations_evaluated");
        builder.Property(r => r.ResolvedCount).IsRequired().HasColumnName("resolved_count");
        builder.Property(r => r.SuppressedCount).IsRequired().HasColumnName("suppressed_count");
        builder.Property(r => r.GappedCount).IsRequired().HasColumnName("gapped_count");
        builder.Property(r => r.FailureReason)
            .HasMaxLength(2048)
            .HasColumnName("failure_reason");

        // Owned collection of outcomes.
        builder.OwnsMany(r => r.Outcomes, outcomes =>
        {
            outcomes.ToTable("reconciliation_outcomes");
            outcomes.WithOwner().HasForeignKey("reconciliation_run_id");

            outcomes.Property<int>("id").ValueGeneratedOnAdd();
            outcomes.HasKey("id");

            outcomes.Property(o => o.LearnerId).IsRequired().HasColumnName("learner_id");
            outcomes.Property(o => o.AssignmentId).IsRequired().HasColumnName("assignment_id");
            outcomes.Property(o => o.Outcome)
                .IsRequired()
                .HasConversion<int>()
                .HasColumnName("outcome");
            outcomes.Property(o => o.CustodyEventId).HasColumnName("custody_event_id");
            outcomes.Property(o => o.GapId).HasColumnName("gap_id");

            outcomes.HasIndex("reconciliation_run_id")
                .HasDatabaseName("ix_reconciliation_outcomes_run");
        });

        builder.HasIndex(r => new { r.SchoolId, r.SchoolDay })
            .HasDatabaseName("ix_reconciliation_runs_school_day");

        builder.Ignore(r => r.DomainEvents);
    }
}