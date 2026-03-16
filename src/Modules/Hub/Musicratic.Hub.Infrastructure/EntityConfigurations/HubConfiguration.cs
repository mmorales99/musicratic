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
            .HasDefaultValue(false);

        builder.Property(h => h.QrUrl)
            .HasColumnName("qr_url")
            .HasMaxLength(2048);

        builder.Property(h => h.DirectLink)
            .HasColumnName("direct_link")
            .HasMaxLength(2048);

        builder.Property(h => h.Visibility)
            .HasColumnName("visibility")
            .HasConversion<string>();

        builder.OwnsOne(h => h.Settings, settings =>
        {
            settings.ToJson("settings_json");
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

        builder.Property(h => h.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(h => h.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(h => h.DomainEvents);
    }
}
