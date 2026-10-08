using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SafePick.Infrastructure.Persistence.Outbox;

namespace SafePick.Infrastructure.Persistence.Configurations;

public sealed class OutboxMessageConfiguration
    : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("outbox_messages");

        builder.HasKey(o => o.Id);

        builder.Property(o => o.Id).HasColumnName("id");
        builder.Property(o => o.EventType)
            .IsRequired()
            .HasMaxLength(512)
            .HasColumnName("event_type");
        builder.Property(o => o.Payload)
            .IsRequired()
            .HasColumnType("text")
            .HasColumnName("payload");
        builder.Property(o => o.OccurredAt)
            .IsRequired()
            .HasColumnName("occurred_at");
        builder.Property(o => o.DispatchedAt)
            .HasColumnName("dispatched_at");
        builder.Property(o => o.Attempts)
            .IsRequired()
            .HasColumnName("attempts");
        builder.Property(o => o.LastError)
            .HasMaxLength(2048)
            .HasColumnName("last_error");

        // The hot path: undispatched messages, oldest first.
        builder.HasIndex(o => new { o.DispatchedAt, o.OccurredAt })
            .HasDatabaseName("ix_outbox_undispatched");
    }
}