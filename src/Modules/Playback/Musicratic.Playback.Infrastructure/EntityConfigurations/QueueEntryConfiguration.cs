using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Playback.Domain.Entities;
using Musicratic.Playback.Domain.Enums;

namespace Musicratic.Playback.Infrastructure.EntityConfigurations;

public sealed class QueueEntryConfiguration : IEntityTypeConfiguration<QueueEntry>
{
    public void Configure(EntityTypeBuilder<QueueEntry> builder)
    {
        builder.ToTable("queue_entries", "playback");

        builder.HasKey(q => q.Id)
            .HasName("pk_queue_entries");

        builder.Property(q => q.Id)
            .HasColumnName("id");

        builder.Property(q => q.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(q => q.TenantId)
            .HasDatabaseName("ix_queue_entries_tenant_id");

        builder.Property(q => q.TrackId)
            .HasColumnName("track_id")
            .IsRequired();

        builder.Property(q => q.HubId)
            .HasColumnName("hub_id")
            .IsRequired();

        builder.HasIndex(q => q.HubId)
            .HasDatabaseName("ix_queue_entries_hub_id");

        builder.HasIndex(q => new { q.HubId, q.Position })
            .HasDatabaseName("ix_queue_entries_hub_position");

        builder.Property(q => q.Position)
            .HasColumnName("position")
            .IsRequired();

        builder.Property(q => q.Status)
            .HasColumnName("status")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(q => q.Source)
            .HasColumnName("source")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(q => q.ProposerId)
            .HasColumnName("proposer_id");

        builder.Property(q => q.CostPaid)
            .HasColumnName("cost_paid")
            .HasDefaultValue(0);

        builder.Property(q => q.StartedAt)
            .HasColumnName("started_at");

        builder.Property(q => q.EndedAt)
            .HasColumnName("ended_at");

        builder.Property(q => q.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(q => q.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(q => q.DomainEvents);
    }
}
