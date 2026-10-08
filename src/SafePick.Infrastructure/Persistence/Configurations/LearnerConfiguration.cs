using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafePick.Domain.Transport;

namespace SafePick.Infrastructure.Persistence.Configurations;

public sealed class LearnerConfiguration : IEntityTypeConfiguration<Learner>
{
    public void Configure(EntityTypeBuilder<Learner> builder)
    {
        builder.ToTable("learners");

        builder.HasKey(l => l.Id);

        builder.Property(l => l.SchoolId).IsRequired();
        builder.Property(l => l.FullName).IsRequired().HasMaxLength(128);
        builder.Property(l => l.IsActive).IsRequired();

        // Owned GradeClass value object → columns class_grade, class_letter
        builder.OwnsOne(l => l.ClassGroup, gc =>
        {
            gc.Property(c => c.Grade)
                .IsRequired()
                .HasColumnName("class_grade");

            gc.Property(c => c.Letter)
                .IsRequired()
                .HasColumnName("class_letter")
                .HasMaxLength(1);
        });

        // Owned optional ParentContact value object
        builder.OwnsOne(l => l.ParentContact, pc =>
        {
            pc.Property(p => p.Email).HasColumnName("parent_email").HasMaxLength(256);
            pc.Property(p => p.Phone).HasColumnName("parent_phone").HasMaxLength(32);
            pc.Property(p => p.Name).HasColumnName("parent_name").HasMaxLength(128);
            pc.Property(p => p.ConsentRecorded).HasColumnName("parent_consent");
        });

        // Composite index on the direct properties that queries filter by.
        // The owned ClassGroup columns (class_grade, class_letter) can be
        // added to this index in a later iteration if query performance
        // demands it; for now, SchoolId + IsActive covers the hot path.
        builder.HasIndex(l => new { l.SchoolId, l.IsActive });

        builder.Ignore(l => l.DomainEvents);
    }
}