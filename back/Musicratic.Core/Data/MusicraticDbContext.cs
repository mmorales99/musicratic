using Microsoft.EntityFrameworkCore;

namespace Musicratic.Core.Data;

// ── Persistence entities (flat, EF-friendly) ─────────────────────────

public class UserEntity
{
    public string Id { get; set; } = string.Empty;
}

public class HubEntity
{
    public string Id { get; set; } = string.Empty;
    public string OwnerId { get; set; } = string.Empty;
    public bool IsPublic { get; set; }
    public double FadeOutDurationSeconds { get; set; } = 10;
    public string PlaybackState { get; set; } = "Stopped";
    public string? PlayingTrackId { get; set; }
    public double PlayingTrackPositionSeconds { get; set; }

    public List<TrackEntity> Tracks { get; set; } = [];
    public List<HubBannedTrackEntity> BannedTracks { get; set; } = [];
    public List<HubUserEntity> AttachedUsers { get; set; } = [];
}

public class TrackEntity
{
    public string Id { get; set; } = string.Empty;
    public string HubId { get; set; } = string.Empty;
    public double DurationSeconds { get; set; }
}

public class HubBannedTrackEntity
{
    public string HubId { get; set; } = string.Empty;
    public string TrackId { get; set; } = string.Empty;
}

public class HubUserEntity
{
    public string HubId { get; set; } = string.Empty;
    public string UserId { get; set; } = string.Empty;
}

// ── DbContext ─────────────────────────────────────────────────────────

public class MusicraticDbContext(DbContextOptions<MusicraticDbContext> options) : DbContext(options)
{
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<HubEntity> Hubs => Set<HubEntity>();
    public DbSet<TrackEntity> Tracks => Set<TrackEntity>();
    public DbSet<HubBannedTrackEntity> HubBannedTracks => Set<HubBannedTrackEntity>();
    public DbSet<HubUserEntity> HubUsers => Set<HubUserEntity>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<UserEntity>().HasKey(u => u.Id);

        b.Entity<HubEntity>().HasKey(h => h.Id);

        b.Entity<TrackEntity>().HasKey(t => new { t.HubId, t.Id });
        b.Entity<TrackEntity>()
            .HasOne<HubEntity>()
            .WithMany(h => h.Tracks)
            .HasForeignKey(t => t.HubId);

        b.Entity<HubBannedTrackEntity>().HasKey(x => new { x.HubId, x.TrackId });
        b.Entity<HubBannedTrackEntity>()
            .HasOne<HubEntity>()
            .WithMany(h => h.BannedTracks)
            .HasForeignKey(x => x.HubId);

        b.Entity<HubUserEntity>().HasKey(x => new { x.HubId, x.UserId });
        b.Entity<HubUserEntity>()
            .HasOne<HubEntity>()
            .WithMany(h => h.AttachedUsers)
            .HasForeignKey(x => x.HubId);
    }
}
