using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Hub.Domain.Entities;

namespace Musicratic.Hub.Infrastructure.EntityConfigurations;

public sealed class HubMemberConfiguration : IEntityTypeConfiguration<HubMember>
{
    public void Configure(EntityTypeBuilder<HubMember> builder)
    {
        builder.ToTable("hub_members", "hub");

        builder.HasKey(m => m.Id)
            .HasName("pk_hub_members");

        builder.Property(m => m.Id)
            .HasColumnName("id");

        builder.Property(m => m.HubId)
            .HasColumnName("hub_id")
            .IsRequired();

        builder.HasIndex(m => m.HubId)
            .HasDatabaseName("ix_hub_members_hub_id");

        builder.Property(m => m.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.HasIndex(m => new { m.HubId, m.UserId })
            .IsUnique()
            .HasDatabaseName("ix_hub_members_hub_id_user_id");

        builder.Property(m => m.Role)
            .HasColumnName("role")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(m => m.AssignedBy)
            .HasColumnName("assigned_by");

        builder.Property(m => m.AssignedAt)
            .HasColumnName("assigned_at")
            .IsRequired();

        builder.Property(m => m.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(m => m.TenantId)
            .HasDatabaseName("ix_hub_members_tenant_id");

        builder.Property(m => m.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(m => m.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(m => m.DomainEvents);
    }
}
