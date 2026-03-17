using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Analytics.Domain.Entities;

namespace Musicratic.Analytics.Infrastructure.EntityConfigurations;

/// <summary>
/// ANLT-002: EF Core configuration for TrackStats entity.
/// Table: track_stats, schema: analytics.
/// </summary>
public sealed class TrackStatsConfiguration : IEntityTypeConfiguration<TrackStats>
{
    public void Configure(EntityTypeBuilder<TrackStats> builder)
    {
        builder.ToTable("track_stats", "analytics");

        builder.HasKey(t => t.Id)
            .HasName("pk_track_stats");

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(t => t.TenantId)
            .HasDatabaseName("ix_track_stats_tenant_id");

        builder.Property(t => t.TrackId)
            .HasColumnName("track_id")
            .IsRequired();

        builder.Property(t => t.HubId)
            .HasColumnName("hub_id")
            .IsRequired();

        // Unique composite index: one stats record per track per hub per tenant
        builder.HasIndex(t => new { t.TrackId, t.HubId, t.TenantId })
            .IsUnique()
            .HasDatabaseName("ix_track_stats_track_id_hub_id_tenant_id");

        builder.HasIndex(t => t.HubId)
            .HasDatabaseName("ix_track_stats_hub_id");

        builder.Property(t => t.Upvotes)
            .HasColumnName("upvotes")
            .IsRequired();

        builder.Property(t => t.Downvotes)
            .HasColumnName("downvotes")
            .IsRequired();

        builder.Property(t => t.Plays)
            .HasColumnName("plays")
            .IsRequired();

        builder.Property(t => t.Skips)
            .HasColumnName("skips")
            .IsRequired();

        builder.Property(t => t.TotalPlayDuration)
            .HasColumnName("total_play_duration")
            .IsRequired();

        builder.Property(t => t.LastPlayedAt)
            .HasColumnName("last_played_at");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore(t => t.DomainEvents);
    }
}
