using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Hub.Domain.Entities;

namespace Musicratic.Hub.Infrastructure.EntityConfigurations;

public sealed class MemberListAssignmentConfiguration : IEntityTypeConfiguration<MemberListAssignment>
{
    public void Configure(EntityTypeBuilder<MemberListAssignment> builder)
    {
        builder.ToTable("member_list_assignments", "hub");

        builder.HasKey(a => a.Id)
            .HasName("pk_member_list_assignments");

        builder.Property(a => a.Id)
            .HasColumnName("id");

        builder.Property(a => a.MemberId)
            .HasColumnName("member_id")
            .IsRequired();

        builder.Property(a => a.ListId)
            .HasColumnName("list_id")
            .IsRequired();

        builder.HasIndex(a => new { a.MemberId, a.ListId })
            .IsUnique()
            .HasDatabaseName("ix_member_list_assignments_member_list");

        builder.Property(a => a.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(a => a.TenantId)
            .HasDatabaseName("ix_member_list_assignments_tenant_id");

        builder.Property(a => a.AssignedBy)
            .HasColumnName("assigned_by")
            .IsRequired();

        builder.Property(a => a.AssignedAt)
            .HasColumnName("assigned_at")
            .IsRequired();

        builder.Property(a => a.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(a => a.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.HasOne(a => a.Member)
            .WithMany()
            .HasForeignKey(a => a.MemberId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.List)
            .WithMany()
            .HasForeignKey(a => a.ListId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Ignore(a => a.DomainEvents);
    }
}
