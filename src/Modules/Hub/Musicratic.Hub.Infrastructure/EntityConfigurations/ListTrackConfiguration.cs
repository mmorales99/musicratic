using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Hub.Domain.Entities;

namespace Musicratic.Hub.Infrastructure.EntityConfigurations;

public sealed class ListTrackConfiguration : IEntityTypeConfiguration<ListTrack>
{
    public void Configure(EntityTypeBuilder<ListTrack> builder)
    {
        builder.ToTable("list_tracks", "hub");

        builder.HasKey(lt => lt.Id)
            .HasName("pk_list_tracks");

        builder.Property(lt => lt.Id)
            .HasColumnName("id");

        builder.Property(lt => lt.ListId)
            .HasColumnName("list_id")
            .IsRequired();

        builder.HasIndex(lt => lt.ListId)
            .HasDatabaseName("ix_list_tracks_list_id");

        builder.Property(lt => lt.TrackId)
            .HasColumnName("track_id")
            .IsRequired();

        builder.HasIndex(lt => new { lt.ListId, lt.TrackId })
            .IsUnique()
            .HasDatabaseName("ix_list_tracks_list_id_track_id");

        builder.Property(lt => lt.Position)
            .HasColumnName("position")
            .IsRequired();

        builder.Property(lt => lt.AddedAt)
            .HasColumnName("added_at")
            .IsRequired();

        builder.Property(lt => lt.TotalUpvotes)
            .HasColumnName("total_upvotes")
            .HasDefaultValue(0);

        builder.Property(lt => lt.TotalDownvotes)
            .HasColumnName("total_downvotes")
            .HasDefaultValue(0);

        builder.Property(lt => lt.TotalPlays)
            .HasColumnName("total_plays")
            .HasDefaultValue(0);

        builder.Property(lt => lt.ShuffleWeight)
            .HasColumnName("shuffle_weight")
            .HasDefaultValue(0.5);

        builder.Property(lt => lt.TenantId)
            .HasColumnName("tenant_id")
            .IsRequired();

        builder.HasIndex(lt => lt.TenantId)
            .HasDatabaseName("ix_list_tracks_tenant_id");

        builder.Property(lt => lt.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(lt => lt.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(lt => lt.DomainEvents);
    }
}
