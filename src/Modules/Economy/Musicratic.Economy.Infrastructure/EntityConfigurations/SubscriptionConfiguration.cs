using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Economy.Domain.Entities;
using Musicratic.Economy.Domain.Enums;

namespace Musicratic.Economy.Infrastructure.EntityConfigurations;

/// <summary>
/// ECON-012: Subscription entity EF configuration.
/// Table: subscriptions in schema economy.
/// Unique index on hub_id (one subscription per hub per tenant).
/// </summary>
public sealed class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions", "economy");

        builder.HasKey(s => s.Id)
            .HasName("pk_subscriptions");

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(s => s.TenantId)
            .HasDatabaseName("ix_subscriptions_tenant_id");

        builder.Property(s => s.HubId)
            .HasColumnName("hub_id")
            .IsRequired();

        builder.HasIndex(s => new { s.HubId, s.TenantId })
            .IsUnique()
            .HasDatabaseName("ix_subscriptions_hub_id_tenant_id");

        builder.Property(s => s.Tier)
            .HasColumnName("tier")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(s => s.StartedAt)
            .HasColumnName("started_at")
            .IsRequired();

        builder.Property(s => s.ExpiresAt)
            .HasColumnName("expires_at");

        builder.Property(s => s.StripeSubscriptionId)
            .HasColumnName("stripe_subscription_id")
            .HasMaxLength(255);

        builder.Property(s => s.TrialEndsAt)
            .HasColumnName("trial_ends_at");

        builder.Property(s => s.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
