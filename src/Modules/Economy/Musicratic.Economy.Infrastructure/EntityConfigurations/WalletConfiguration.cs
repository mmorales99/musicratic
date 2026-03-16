using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Economy.Domain.Entities;

namespace Musicratic.Economy.Infrastructure.EntityConfigurations;

public sealed class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.ToTable("wallets", "economy");

        builder.HasKey(w => w.Id)
            .HasName("pk_wallets");

        builder.Property(w => w.Id)
            .HasColumnName("id");

        builder.Property(w => w.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(w => w.TenantId)
            .HasDatabaseName("ix_wallets_tenant_id");

        builder.Property(w => w.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(w => new { w.UserId, w.TenantId })
            .IsUnique()
            .HasDatabaseName("ix_wallets_user_id_tenant_id");

        builder.Property(w => w.Balance)
            .HasColumnName("balance")
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(w => w.Currency)
            .HasColumnName("currency")
            .HasMaxLength(20)
            .HasDefaultValue("MUS_COIN")
            .IsRequired();

        builder.Property(w => w.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(w => w.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasMany(w => w.Transactions)
            .WithOne()
            .HasForeignKey(t => t.WalletId)
            .HasConstraintName("fk_transactions_wallet_id")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Metadata.FindNavigation(nameof(Wallet.Transactions))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);
    }
}
