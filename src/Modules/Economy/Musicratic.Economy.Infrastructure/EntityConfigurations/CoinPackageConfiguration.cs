using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Economy.Domain.Entities;

namespace Musicratic.Economy.Infrastructure.EntityConfigurations;

/// <summary>
/// Supra-tenant entity — no tenant filter applied.
/// </summary>
public sealed class CoinPackageConfiguration : IEntityTypeConfiguration<CoinPackage>
{
    public void Configure(EntityTypeBuilder<CoinPackage> builder)
    {
        builder.ToTable("coin_packages", "economy");

        builder.HasKey(p => p.Id)
            .HasName("pk_coin_packages");

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(p => p.CoinAmount)
            .HasColumnName("coin_amount")
            .IsRequired();

        builder.Property(p => p.PriceUsd)
            .HasColumnName("price_usd")
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(p => p.BonusCoins)
            .HasColumnName("bonus_coins")
            .HasDefaultValue(0)
            .IsRequired();

        builder.Property(p => p.IsActive)
            .HasColumnName("is_active")
            .HasDefaultValue(true)
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
