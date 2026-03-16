using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Hub.Domain.Entities;

namespace Musicratic.Hub.Infrastructure.EntityConfigurations;

public sealed class HubAttachmentConfiguration : IEntityTypeConfiguration<HubAttachment>
{
    public void Configure(EntityTypeBuilder<HubAttachment> builder)
    {
        builder.ToTable("hub_attachments", "hub");

        builder.HasKey(a => a.Id)
            .HasName("pk_hub_attachments");

        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(a => a.UserId)
            .HasDatabaseName("ix_hub_attachments_user_id");

        builder.Property(a => a.HubId)
            .HasColumnName("hub_id")
            .IsRequired();

        builder.Property(a => a.AttachedAt)
            .HasColumnName("attached_at")
            .IsRequired();

        builder.Property(a => a.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(a => a.EndedAt)
            .HasColumnName("ended_at");

        builder.Ignore(a => a.IsActive);

        builder.Property(a => a.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(a => a.TenantId)
            .HasDatabaseName("ix_hub_attachments_tenant_id");

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(a => a.DomainEvents);
    }
}
