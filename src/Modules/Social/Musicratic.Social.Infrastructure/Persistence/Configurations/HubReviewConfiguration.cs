using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Social.Domain.Entities;

namespace Musicratic.Social.Infrastructure.Persistence.Configurations;

public sealed class HubReviewConfiguration : IEntityTypeConfiguration<HubReview>
{
    public void Configure(EntityTypeBuilder<HubReview> builder)
    {
        builder.ToTable("hub_reviews", "social", t =>
        {
            t.HasCheckConstraint("ck_hub_reviews_rating", "rating >= 1 AND rating <= 5");
        });

        builder.HasKey(r => r.Id)
            .HasName("pk_hub_reviews");

        builder.Property(r => r.Id)
            .HasColumnName("id");

        builder.Property(r => r.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(r => r.TenantId)
            .HasDatabaseName("ix_hub_reviews_tenant_id");

        builder.Property(r => r.HubId)
            .HasColumnName("hub_id")
            .IsRequired();

        builder.HasIndex(r => r.HubId)
            .HasDatabaseName("ix_hub_reviews_hub_id");

        builder.HasIndex(r => new { r.HubId, r.UserId })
            .IsUnique()
            .HasDatabaseName("ix_hub_reviews_hub_id_user_id");

        builder.Property(r => r.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(r => r.Rating)
            .HasColumnName("rating")
            .IsRequired();

        builder.Property(r => r.Comment)
            .HasColumnName("comment")
            .HasMaxLength(500);

        builder.Property(r => r.OwnerResponse)
            .HasColumnName("owner_response")
            .HasMaxLength(500);

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(r => r.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(r => r.DomainEvents);
    }
}
