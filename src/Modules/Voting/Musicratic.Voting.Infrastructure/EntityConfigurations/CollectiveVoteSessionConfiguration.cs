using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Voting.Domain.Entities;
using Musicratic.Voting.Domain.Enums;

namespace Musicratic.Voting.Infrastructure.EntityConfigurations;

public sealed class CollectiveVoteSessionConfiguration
    : IEntityTypeConfiguration<CollectiveVoteSession>
{
    public void Configure(EntityTypeBuilder<CollectiveVoteSession> builder)
    {
        builder.ToTable("collective_vote_sessions", "voting");

        builder.HasKey(s => s.Id)
            .HasName("pk_collective_vote_sessions");

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(s => s.TenantId)
            .HasDatabaseName("ix_collective_vote_sessions_tenant_id");

        builder.Property(s => s.QueueEntryId)
            .HasColumnName("queue_entry_id")
            .IsRequired();

        builder.HasIndex(s => s.QueueEntryId)
            .HasDatabaseName("ix_collective_vote_sessions_queue_entry_id");

        builder.Property(s => s.ProposerId)
            .HasColumnName("proposer_id")
            .IsRequired();

        builder.HasIndex(s => new { s.TenantId, s.ProposerId })
            .HasDatabaseName("ix_collective_vote_sessions_tenant_proposer");

        builder.Property(s => s.Status)
            .HasColumnName("status")
            .HasConversion<int>()
            .IsRequired();

        builder.Property(s => s.OpensAt)
            .HasColumnName("opens_at")
            .IsRequired();

        builder.Property(s => s.ExpiresAt)
            .HasColumnName("expires_at")
            .IsRequired();

        builder.Property(s => s.RequiredApprovalPercentage)
            .HasColumnName("required_approval_percentage")
            .IsRequired();

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at");

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        builder.Ignore(s => s.DomainEvents);
        builder.Ignore(s => s.IsExpired);
    }
}
