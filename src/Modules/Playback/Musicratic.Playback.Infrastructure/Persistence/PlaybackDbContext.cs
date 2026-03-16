using Microsoft.EntityFrameworkCore;
using Musicratic.Playback.Domain.Entities;
using Musicratic.Shared.Infrastructure;
using Musicratic.Shared.Infrastructure.Persistence;

namespace Musicratic.Playback.Infrastructure.Persistence;

public sealed class PlaybackDbContext : BaseDbContext
{
    public DbSet<Track> Tracks => Set<Track>();
    public DbSet<QueueEntry> QueueEntries => Set<QueueEntry>();

    public PlaybackDbContext(
        DbContextOptions<PlaybackDbContext> options,
        TenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("playback");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(PlaybackDbContext).Assembly);
    }
}
