using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafePick.Domain.Transport;

namespace SafePick.Infrastructure.Persistence.Configurations;

public sealed class DriverApprovalConfiguration : IEntityTypeConfiguration<DriverApproval>
{
    public void Configure(EntityTypeBuilder<DriverApproval> builder)
    {
        builder.ToTable("driver_approvals");

        builder.HasKey(a => a.Id);

        builder.Property(a => a.DriverId).IsRequired();
        builder.Property(a => a.SchoolId).IsRequired();
        builder.Property(a => a.Status).IsRequired().HasConversion<int>();
        builder.Property(a => a.DecidedByUserId).IsRequired(false);
        builder.Property(a => a.DecidedAt).IsRequired(false);
        builder.Property(a => a.Reason).HasMaxLength(512).IsRequired(false);
        builder.Property(a => a.CreatedAt).IsRequired();

        // One driver can have at most one *Pending* approval per school.
        // We enforce that in the application layer, not the schema —
        // historical approvals (Rejected, Revoked) must coexist.
        builder.HasIndex(a => new { a.DriverId, a.SchoolId });

        builder.Ignore(a => a.DomainEvents);
    }
}