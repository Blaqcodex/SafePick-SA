using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafePick.Domain.Transport;

namespace SafePick.Infrastructure.Persistence.Configurations;

public sealed class AssignmentConfiguration : IEntityTypeConfiguration<Assignment>
{
    public void Configure(EntityTypeBuilder<Assignment> builder)
    {
        builder.ToTable("assignments");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.Id).HasColumnName("id");
        builder.Property(a => a.SchoolId).IsRequired().HasColumnName("school_id");
        builder.Property(a => a.LearnerId).IsRequired().HasColumnName("learner_id");
        builder.Property(a => a.VehicleId).IsRequired().HasColumnName("vehicle_id");
        builder.Property(a => a.DriverId).IsRequired().HasColumnName("driver_id");
        builder.Property(a => a.ValidFrom)
            .IsRequired()
            .HasColumnName("valid_from")
            .HasColumnType("timestamp with time zone");
        builder.Property(a => a.ValidTo)
            .IsRequired(false)
            .HasColumnName("valid_to")
            .HasColumnType("timestamp with time zone");
        builder.Property(a => a.AssignedByUserId).IsRequired().HasColumnName("assigned_by_user_id");

        builder.HasIndex(a => new { a.SchoolId, a.LearnerId, a.ValidTo })
            .HasDatabaseName("ix_assignments_school_learner_valid_to");

        builder.HasIndex(a => new { a.VehicleId, a.ValidTo })
            .HasDatabaseName("ix_assignments_vehicle_valid_to");

        builder.Ignore(a => a.DomainEvents);
    }
}