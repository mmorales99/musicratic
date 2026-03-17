using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Analytics.Domain.Entities;

namespace Musicratic.Analytics.Infrastructure.EntityConfigurations;

/// <summary>
/// ANLT-002: EF Core configuration for HubStats entity.
/// Table: hub_stats, schema: analytics.
/// </summary>
public sealed class HubStatsConfiguration : IEntityTypeConfiguration<HubStats>
{
    public void Configure(EntityTypeBuilder<HubStats> builder)
    {
        builder.ToTable("hub_stats", "analytics");

        builder.HasKey(h => h.Id)
            .HasName("pk_hub_stats");

        builder.Property(h => h.Id)
            .HasColumnName("id");

        builder.Property(h => h.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(h => h.TenantId)
            .HasDatabaseName("ix_hub_stats_tenant_id");

        builder.Property(h => h.HubId)
            .HasColumnName("hub_id")
            .IsRequired();

        // Unique composite index: one stats record per hub per tenant
        builder.HasIndex(h => new { h.HubId, h.TenantId })
            .IsUnique()
            .HasDatabaseName("ix_hub_stats_hub_id_tenant_id");

        builder.Property(h => h.TotalPlays)
            .HasColumnName("total_plays")
            .IsRequired();

        builder.Property(h => h.TotalVotes)
            .HasColumnName("total_votes")
            .IsRequired();

        builder.Property(h => h.UniqueListeners)
            .HasColumnName("unique_listeners")
            .IsRequired();

        builder.Property(h => h.ActiveHours)
            .HasColumnName("active_hours")
            .IsRequired();

        builder.Property(h => h.PeakConcurrentUsers)
            .HasColumnName("peak_concurrent_users")
            .IsRequired();

        builder.Property(h => h.LastActivityAt)
            .HasColumnName("last_activity_at");

        builder.Property(h => h.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(h => h.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore(h => h.DomainEvents);
    }
}
