using Microsoft.EntityFrameworkCore;
using Musicratic.Core.Models;
using Musicratic.Core.Ports;

namespace Musicratic.Core.Data;

public class EfHubRepository(MusicraticDbContext db) : IHubRepository
{
    public async Task<Hub?> GetByIdAsync(string id)
    {
        var entity = await db.Hubs
            .Include(h => h.Tracks)
            .Include(h => h.BannedTracks)
            .Include(h => h.AttachedUsers)
            .FirstOrDefaultAsync(h => h.Id == id);
        return entity is null ? null : Mapper.ToDomain(entity);
    }

    public async Task AddAsync(Hub hub)
    {
        db.Hubs.Add(Mapper.ToEntity(hub));
        await db.SaveChangesAsync();
    }

    public async Task SaveAsync(Hub hub)
    {
        var entity = await db.Hubs
            .Include(h => h.Tracks)
            .Include(h => h.BannedTracks)
            .Include(h => h.AttachedUsers)
            .FirstOrDefaultAsync(h => h.Id == hub.Id);
        if (entity is null) return;

        entity.IsPublic = hub.IsPublic;
        entity.FadeOutDurationSeconds = hub.Config.FadeOutDuration.TotalSeconds;
        entity.PlaybackState = hub.State.ToString();
        entity.PlayingTrackId = hub.PlayingTrack?.Id;
        entity.PlayingTrackPositionSeconds = hub.PlayingTrackPosition.TotalSeconds;

        // Replace child collections atomically
        db.Tracks.RemoveRange(entity.Tracks);
        db.HubBannedTracks.RemoveRange(entity.BannedTracks);
        db.HubUsers.RemoveRange(entity.AttachedUsers);
        db.Tracks.AddRange(hub.Tracks.Select(t => Mapper.ToTrackEntity(t, hub.Id)));
        db.HubBannedTracks.AddRange(hub.BannedTracks
            .Select(t => new HubBannedTrackEntity { HubId = hub.Id, TrackId = t.Id }));
        db.HubUsers.AddRange(hub.AttachedUsers
            .Select(u => new HubUserEntity { HubId = hub.Id, UserId = u.Id }));

        await db.SaveChangesAsync();
    }
}
