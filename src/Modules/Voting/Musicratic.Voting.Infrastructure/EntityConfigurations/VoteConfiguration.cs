using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Voting.Domain.Entities;
using Musicratic.Voting.Domain.Enums;

namespace Musicratic.Voting.Infrastructure.EntityConfigurations;

public sealed class VoteConfiguration : IEntityTypeConfiguration<Vote>
{
    public void Configure(EntityTypeBuilder<Vote> builder)
    {
        builder.ToTable("votes", "voting");

        builder.HasKey(v => v.Id)
            .HasName("pk_votes");

        builder.Property(v => v.Id)
            .HasColumnName("id");

        builder.Property(v => v.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(v => v.TenantId)
            .HasDatabaseName("ix_votes_tenant_id");

        builder.Property(v => v.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(v => v.QueueEntryId)
            .HasColumnName("queue_entry_id")
            .IsRequired();

        // One vote per user per queue entry — spec: "One vote per user per queue entry."
        builder.HasIndex(v => new { v.UserId, v.QueueEntryId })
            .IsUnique()
            .HasDatabaseName("ix_votes_user_id_queue_entry_id");

        builder.Property(v => v.Value)
            .HasColumnName("value")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(v => v.CastAt)
            .HasColumnName("cast_at")
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(v => v.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore(v => v.DomainEvents);
    }
}
