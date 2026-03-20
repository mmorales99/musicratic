using Musicratic.Core.Models;

namespace Musicratic.Core.Data;

public static class Mapper
{
    // ── To domain ────────────────────────────────────────────────────

    public static Models.User ToDomain(UserEntity e) => new() { Id = e.Id };

    public static Models.Hub ToDomain(HubEntity e)
    {
        var tracks = e.Tracks.Select(TrackToDomain).ToList();

        var hub = new Models.Hub
        {
            Id = e.Id,
            Owner = new Owner { Id = e.OwnerId },
            IsPublic = e.IsPublic,
            Config = new HubConfig
            {
                FadeOutDuration = TimeSpan.FromSeconds(e.FadeOutDurationSeconds)
            },
            Tracks = tracks,
            BannedTracks = e.BannedTracks
                .Select(b => tracks.FirstOrDefault(t => t.Id == b.TrackId))
                .Where(t => t is not null)
                .Select(t => t!)
                .ToList(),
            AttachedUsers = e.AttachedUsers.Select(u => new Models.User { Id = u.UserId }).ToList(),
        };

        hub.Queue = [.. tracks];
        hub.SuggestedTracks = [];

        // Restore persisted playback state
        if (Enum.TryParse<Models.PlaybackState>(e.PlaybackState, out var state))
            hub.State = state;
        hub.PlayingTrackPosition = TimeSpan.FromSeconds(e.PlayingTrackPositionSeconds);
        if (e.PlayingTrackId is not null)
            hub.PlayingTrack = tracks.FirstOrDefault(t => t.Id == e.PlayingTrackId);

        return hub;
    }

    public static Models.Track TrackToDomain(TrackEntity e) =>
        new() { Id = e.Id, Duration = TimeSpan.FromSeconds(e.DurationSeconds) };

    // ── To entities ──────────────────────────────────────────────────

    public static UserEntity ToEntity(Models.User u) => new() { Id = u.Id };

    public static HubEntity ToEntity(Models.Hub h) => new()
    {
        Id = h.Id,
        OwnerId = h.Owner.Id,
        IsPublic = h.IsPublic,
        FadeOutDurationSeconds = h.Config.FadeOutDuration.TotalSeconds,
        PlaybackState = h.State.ToString(),
        PlayingTrackId = h.PlayingTrack?.Id,
        PlayingTrackPositionSeconds = h.PlayingTrackPosition.TotalSeconds,
    };

    public static TrackEntity ToTrackEntity(Models.Track t, string hubId) => new()
    {
        Id = t.Id,
        HubId = hubId,
        DurationSeconds = t.Duration.TotalSeconds,
    };
}
