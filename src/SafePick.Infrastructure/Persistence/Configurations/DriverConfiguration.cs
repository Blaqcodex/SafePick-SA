using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafePick.Domain.Transport;

namespace SafePick.Infrastructure.Persistence.Configurations;

public sealed class DriverConfiguration : IEntityTypeConfiguration<Driver>
{
    public void Configure(EntityTypeBuilder<Driver> builder)
    {
        builder.ToTable("drivers");

        builder.HasKey(d => d.Id);

        builder.Property(d => d.FullName).IsRequired().HasMaxLength(128);
        builder.Property(d => d.IdNumber).IsRequired().HasMaxLength(32);
        builder.Property(d => d.Phone).IsRequired().HasMaxLength(32);
        builder.Property(d => d.Email).IsRequired().HasMaxLength(256);
        builder.Property(d => d.IsActive).IsRequired();

        // Owned single Vehicle entity
        builder.OwnsOne(d => d.Vehicle, vehicle =>
        {
            vehicle.ToTable("vehicles");
            vehicle.WithOwner().HasForeignKey("driver_id");
            vehicle.HasKey(v => v.Id);

            vehicle.Property(v => v.RegistrationNumber)
                .IsRequired()
                .HasMaxLength(32);

            vehicle.HasIndex(v => v.RegistrationNumber).IsUnique();

            vehicle.Property(v => v.Make).IsRequired().HasMaxLength(64);
            vehicle.Property(v => v.Model).IsRequired().HasMaxLength(64);
            vehicle.Property(v => v.Capacity).IsRequired();
        });

        builder.Ignore(d => d.DomainEvents);
        builder.Navigation(d => d.Vehicle).AutoInclude();
    }
}