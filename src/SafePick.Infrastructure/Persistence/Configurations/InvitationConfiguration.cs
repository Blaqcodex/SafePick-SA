using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafePick.Domain.Identity;

namespace SafePick.Infrastructure.Persistence.Configurations;

/// <summary>
/// EF Core mapping for the Invitation aggregate.
///
/// Note: only the *hash* of the token is stored. The raw token value is
/// carried only during creation and during redemption — it is never
/// persisted. This is a security decision, enforced here.
/// </summary>
public sealed class InvitationConfiguration : IEntityTypeConfiguration<Invitation>
{
    public void Configure(EntityTypeBuilder<Invitation> builder)
    {
        builder.ToTable("invitations");

        builder.HasKey(i => i.Id);

        builder.Property(i => i.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(i => i.Email);

        builder.Property(i => i.Role)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.SchoolId)
            .IsRequired();

        // The InvitationToken value object is mapped as a converted string.
        // Only its hash is stored (see the handler that generates it).
        builder.Property(i => i.Token)
            .IsRequired()
            .HasMaxLength(256)
            .HasConversion(
                token => token.Value,
                value => new InvitationToken(value));

        builder.HasIndex(i => i.Token)
            .IsUnique();

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(i => i.IssuedAt)
            .IsRequired();

        builder.Property(i => i.ExpiresAt)
            .IsRequired();

        builder.Property(i => i.AcceptedAt)
            .IsRequired(false);

        builder.Property(i => i.IssuedByUserId)
            .IsRequired();

        builder.Ignore(i => i.DomainEvents);
    }
}