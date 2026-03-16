using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Notification.Domain.Entities;

namespace Musicratic.Notification.Infrastructure.EntityConfigurations;

public sealed class DeviceTokenConfiguration : IEntityTypeConfiguration<DeviceToken>
{
    public void Configure(EntityTypeBuilder<DeviceToken> builder)
    {
        builder.ToTable("device_tokens", "notification");

        builder.HasKey(d => d.Id)
            .HasName("pk_device_tokens");

        builder.Property(d => d.Id)
            .HasColumnName("id");

        builder.Property(d => d.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(d => d.Token)
            .HasColumnName("token")
            .HasMaxLength(512)
            .IsRequired();

        builder.Property(d => d.Platform)
            .HasColumnName("platform")
            .HasConversion<string>()
            .HasMaxLength(20)
            .IsRequired();

        builder.Property(d => d.DeviceName)
            .HasColumnName("device_name")
            .HasMaxLength(200);

        builder.Property(d => d.IsActive)
            .HasColumnName("is_active")
            .IsRequired();

        builder.Property(d => d.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(d => d.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasIndex(d => new { d.UserId, d.Token })
            .IsUnique()
            .HasDatabaseName("ix_device_tokens_user_id_token");

        builder.HasIndex(d => new { d.UserId, d.Platform })
            .HasDatabaseName("ix_device_tokens_user_id_platform");

        builder.Ignore(d => d.DomainEvents);
    }
}
