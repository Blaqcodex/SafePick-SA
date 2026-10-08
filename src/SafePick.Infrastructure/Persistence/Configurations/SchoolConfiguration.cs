using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafePick.Domain.Transport;

namespace SafePick.Infrastructure.Persistence.Configurations;

public sealed class SchoolConfiguration : IEntityTypeConfiguration<School>
{
    public void Configure(EntityTypeBuilder<School> builder)
    {
        builder.ToTable("schools");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.ContactEmail)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(s => s.IsActive)
            .IsRequired();

        // Owned collection of value objects
        builder.OwnsMany(s => s.TransportWindows, windows =>
        {
            windows.ToTable("school_transport_windows");
            windows.WithOwner().HasForeignKey("school_id");
            windows.Property<int>("id").ValueGeneratedOnAdd();
            windows.HasKey("id");

            windows.Property(w => w.OpensAt).IsRequired();
            windows.Property(w => w.ClosesAt).IsRequired();
            windows.Property(w => w.GracePeriod).IsRequired();
        });

        builder.Ignore(s => s.DomainEvents);
        builder.Navigation(s => s.TransportWindows).AutoInclude();
    }
}