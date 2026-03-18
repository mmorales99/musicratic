using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Hub.Domain.Enums;

namespace Musicratic.Hub.Infrastructure.EntityConfigurations;

public sealed class HubConfiguration : IEntityTypeConfiguration<Domain.Entities.Hub>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Hub> builder)
    {
        builder.ToTable("hubs", "hub");

        builder.HasKey(h => h.Id)
            .HasName("pk_hubs");

        builder.Property(h => h.Id)
            .HasColumnName("id");

        builder.Property(h => h.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(h => h.Code)
            .HasColumnName("code")
            .HasMaxLength(20)
            .IsRequired();

        builder.HasIndex(h => h.Code)
            .IsUnique()
            .HasDatabaseName("ix_hubs_code");

        builder.Property(h => h.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(h => h.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(h => h.SubscriptionTier)
            .HasColumnName("subscription_tier")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(h => h.SubscriptionExpiresAt)
            .HasColumnName("subscription_expires_at");

        builder.Property(h => h.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(h => h.IsPaused)
            .HasColumnName("is_paused")
            .IsRequired();

        builder.Property(h => h.PausedAt)
            .HasColumnName("paused_at");

        builder.Property(h => h.QrUrl)
            .HasColumnName("qr_url")
            .HasMaxLength(2048);

        builder.Property(h => h.DirectLink)
            .HasColumnName("direct_link")
            .HasMaxLength(2048);

        builder.Property(h => h.Visibility)
            .HasColumnName("visibility")
            .HasConversion<string>();

        builder.Property(h => h.IsDeleted)
            .HasColumnName("is_deleted")
            .IsRequired();

        builder.Property(h => h.DeletedAt)
            .HasColumnName("deleted_at");

        builder.HasQueryFilter(h => h.IsDeleted == false);

        builder.OwnsOne(h => h.Settings, settings =>
        {
            settings.Property(s => s.AllowProposals)
                .HasColumnName("settings_allow_proposals");

            settings.Property(s => s.AutoSkipThreshold)
                .HasColumnName("settings_auto_skip_threshold");

            settings.Property(s => s.VotingWindowSeconds)
                .HasColumnName("settings_voting_window_seconds");

            settings.Property(s => s.MaxQueueSize)
                .HasColumnName("settings_max_queue_size");

            settings.Property(s => s.AllowedProviders)
                .HasColumnName("settings_allowed_providers")
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                    v => JsonSerializer.Deserialize<List<MusicProvider>>(v, (JsonSerializerOptions?)null) ?? new List<MusicProvider>())
                .HasMaxLength(500);

            settings.Property(s => s.EnableLocalStorage)
                .HasColumnName("settings_enable_local_storage");

            settings.Property(s => s.AdsEnabled)
                .HasColumnName("settings_ads_enabled");
        });

        builder.Property(h => h.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(h => h.TenantId)
            .HasDatabaseName("ix_hubs_tenant_id");

        builder.HasMany(h => h.Members)
            .WithOne(m => m.Hub)
            .HasForeignKey(m => m.HubId)
            .HasConstraintName("fk_hub_members_hubs");

        builder.Navigation(h => h.Members)
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasField("_members");

        builder.Property(h => h.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(h => h.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(h => h.DomainEvents);
    }
}
