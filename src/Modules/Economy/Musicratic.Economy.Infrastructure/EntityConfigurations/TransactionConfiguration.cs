using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Economy.Domain.Entities;
using Musicratic.Economy.Domain.Enums;

namespace Musicratic.Economy.Infrastructure.EntityConfigurations;

public sealed class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("transactions", "economy");

        builder.HasKey(t => t.Id)
            .HasName("pk_transactions");

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(t => t.TenantId)
            .HasDatabaseName("ix_transactions_tenant_id");

        builder.Property(t => t.WalletId)
            .HasColumnName("wallet_id")
            .IsRequired();

        builder.HasIndex(t => t.WalletId)
            .HasDatabaseName("ix_transactions_wallet_id");

        builder.Property(t => t.Amount)
            .HasColumnName("amount")
            .HasPrecision(18, 4)
            .IsRequired();

        builder.Property(t => t.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(t => t.Reason)
            .HasColumnName("reason")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.ReferenceId)
            .HasColumnName("reference_id");

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.HasIndex(t => t.CreatedAt)
            .HasDatabaseName("ix_transactions_created_at");

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();
    }
}
