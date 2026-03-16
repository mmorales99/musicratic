using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Hub.Domain.Entities;

namespace Musicratic.Hub.Infrastructure.EntityConfigurations;

public sealed class ListConfiguration : IEntityTypeConfiguration<List>
{
    public void Configure(EntityTypeBuilder<List> builder)
    {
        builder.ToTable("lists", "hub");

        builder.HasKey(l => l.Id)
            .HasName("pk_lists");

        builder.Property(l => l.Id)
            .HasColumnName("id");

        builder.Property(l => l.Name)
            .HasColumnName("name")
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(l => l.HubId)
            .HasColumnName("hub_id")
            .IsRequired();

        builder.HasIndex(l => l.HubId)
            .HasDatabaseName("ix_lists_hub_id");

        builder.Property(l => l.OwnerId)
            .HasColumnName("owner_id")
            .IsRequired();

        builder.Property(l => l.PlayMode)
            .HasColumnName("play_mode")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(l => l.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(l => l.TenantId)
            .HasDatabaseName("ix_lists_tenant_id");

        builder.HasMany(l => l.Tracks)
            .WithOne(t => t.List)
            .HasForeignKey(t => t.ListId)
            .HasConstraintName("fk_list_tracks_lists");

        builder.Property(l => l.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(l => l.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(l => l.DomainEvents);
    }
}
