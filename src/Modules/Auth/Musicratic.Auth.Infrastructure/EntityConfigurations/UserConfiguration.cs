using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Auth.Domain.Entities;

namespace Musicratic.Auth.Infrastructure.EntityConfigurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("users", "auth");

        builder.HasKey(u => u.Id)
            .HasName("pk_users");

        builder.Property(u => u.Id)
            .HasColumnName("id");

        builder.Property(u => u.AuthentikSub)
            .HasColumnName("authentik_sub")
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(u => u.AuthentikSub)
            .IsUnique()
            .HasDatabaseName("ix_users_authentik_sub");

        builder.Property(u => u.DisplayName)
            .HasColumnName("display_name")
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(u => u.Email)
            .HasColumnName("email")
            .HasMaxLength(320)
            .IsRequired();

        builder.HasIndex(u => u.Email)
            .IsUnique()
            .HasDatabaseName("ix_users_email");

        builder.Property(u => u.AvatarUrl)
            .HasColumnName("avatar_url")
            .HasMaxLength(2048);

        builder.Property(u => u.WalletBalance)
            .HasColumnName("wallet_balance")
            .HasDefaultValue(0);

        builder.Property(u => u.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(u => u.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(u => u.DomainEvents);
    }
}
