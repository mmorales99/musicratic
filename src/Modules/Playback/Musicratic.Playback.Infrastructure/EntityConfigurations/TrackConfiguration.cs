using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Musicratic.Playback.Domain.Entities;
using Musicratic.Playback.Domain.Enums;

namespace Musicratic.Playback.Infrastructure.EntityConfigurations;

public sealed class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.ToTable("tracks", "playback");

        builder.HasKey(t => t.Id)
            .HasName("pk_tracks");

        builder.Property(t => t.Id)
            .HasColumnName("id");

        builder.Property(t => t.Provider)
            .HasColumnName("provider")
            .HasConversion<string>()
            .IsRequired();

        builder.Property(t => t.ExternalId)
            .HasColumnName("external_id")
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(t => new { t.Provider, t.ExternalId })
            .IsUnique()
            .HasDatabaseName("ix_tracks_provider_external_id");

        builder.Property(t => t.Title)
            .HasColumnName("title")
            .HasMaxLength(500)
            .IsRequired();

        builder.Property(t => t.Artist)
            .HasColumnName("artist")
            .HasMaxLength(250)
            .IsRequired();

        builder.Property(t => t.Album)
            .HasColumnName("album")
            .HasMaxLength(250);

        builder.Property(t => t.DurationSeconds)
            .HasColumnName("duration_seconds")
            .IsRequired();

        builder.Property(t => t.AlbumArtUrl)
            .HasColumnName("album_art_url")
            .HasMaxLength(2048);

        builder.Property(t => t.Hotness)
            .HasColumnName("hotness")
            .HasDefaultValue(0.0);

        builder.Property(t => t.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(t => t.UpdatedAt)
            .HasColumnName("updated_at")
            .IsRequired();

        builder.Ignore(t => t.DomainEvents);
    }
}
