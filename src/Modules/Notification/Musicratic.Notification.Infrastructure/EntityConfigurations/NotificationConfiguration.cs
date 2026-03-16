using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Notification.Domain.Entities;

namespace Musicratic.Notification.Infrastructure.EntityConfigurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Domain.Entities.Notification>
{
    public void Configure(EntityTypeBuilder<Domain.Entities.Notification> builder)
    {
        builder.ToTable("notifications", "notification");

        builder.HasKey(n => n.Id)
            .HasName("pk_notifications");

        builder.Property(n => n.Id)
            .HasColumnName("id");

        builder.Property(n => n.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(n => n.UserId)
            .HasDatabaseName("ix_notifications_user_id");

        builder.HasIndex(n => new { n.UserId, n.ReadAt })
            .HasDatabaseName("ix_notifications_user_id_read_at");

        builder.Property(n => n.Type)
            .HasColumnName("type")
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(n => n.Title)
            .HasColumnName("title")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(n => n.Body)
            .HasColumnName("body")
            .HasMaxLength(2000)
            .IsRequired();

        builder.Property(n => n.DataJson)
            .HasColumnName("data_json")
            .HasMaxLength(4000);

        builder.Property(n => n.ReadAt)
            .HasColumnName("read_at");

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(n => n.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(n => n.DomainEvents);
    }
}
