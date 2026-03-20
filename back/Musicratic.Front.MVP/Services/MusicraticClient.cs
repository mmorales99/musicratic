using System.Net.Http.Json;

namespace Musicratic.Front.MVP.Services;

public record HubStatusDto(
    string Id,
    string OwnerId,
    bool IsPublic,
    string PlaybackState,
    string? PlayingTrackId,
    double PlayingTrackPositionSeconds,
    bool IsSuggestedTrackPlaying,
    bool CanSuggest,
    VotingDto? CurrentVoting,
    List<string> QueueTrackIds,
    List<string> SuggestedTrackIds,
    List<string> AttachedUserIds,
    List<string> BannedTrackIds,
    double FadeOutDurationSeconds);

public record VotingDto(
    string TrackId,
    int UpVotes,
    int DownVotes,
    double TimeLeftSeconds,
    string Phase,
    bool IsActive);

public record UserDto(string Id);

public class MusicraticClient(HttpClient http)
{
    // ── Users ────────────────────────────────────────────────────────

    public Task<List<UserDto>?> GetUsersAsync() =>
        http.GetFromJsonAsync<List<UserDto>>("/users");

    public Task<HttpResponseMessage> CreateUserAsync(string userId) =>
        http.PostAsJsonAsync("/users", new { UserId = userId });

    public Task<HttpResponseMessage> DeleteUserAsync(string userId) =>
        http.DeleteAsync($"/users/{userId}");

    // ── Hubs ─────────────────────────────────────────────────────────

    public Task<HubStatusDto?> GetHubAsync(string hubId) =>
        http.GetFromJsonAsync<HubStatusDto>($"/hubs/{hubId}");

    public Task<HttpResponseMessage> CreateHubAsync(string hubId, string ownerId, bool isPublic) =>
        http.PostAsJsonAsync("/hubs", new { HubId = hubId, OwnerId = ownerId, IsPublic = isPublic });

    // ── Tracks ───────────────────────────────────────────────────────

    public Task<HttpResponseMessage> SetTracksAsync(string hubId, List<TrackInput> tracks) =>
        http.PutAsJsonAsync($"/hubs/{hubId}/tracks", new { Tracks = tracks });

    // ── Playback ─────────────────────────────────────────────────────

    public Task<HttpResponseMessage> PlayAsync(string hubId) =>
        http.PostAsync($"/hubs/{hubId}/play", null);

    public Task<HttpResponseMessage> StopAsync(string hubId) =>
        http.PostAsync($"/hubs/{hubId}/stop", null);

    public Task<HttpResponseMessage> NextAsync(string hubId) =>
        http.PostAsync($"/hubs/{hubId}/next", null);

    public Task<HttpResponseMessage> ShuffleAsync(string hubId) =>
        http.PostAsync($"/hubs/{hubId}/shuffle", null);

    // ── Hub users ────────────────────────────────────────────────────

    public Task<HttpResponseMessage> AttachUserAsync(string hubId, string userId) =>
        http.PostAsJsonAsync($"/hubs/{hubId}/users", new { UserId = userId });

    public Task<HttpResponseMessage> DetachUserAsync(string hubId, string userId) =>
        http.DeleteAsync($"/hubs/{hubId}/users/{userId}");

    // ── Ban list ─────────────────────────────────────────────────────

    public Task<HttpResponseMessage> BanTrackAsync(string hubId, string trackId) =>
        http.PostAsJsonAsync($"/hubs/{hubId}/banned-tracks", new { TrackId = trackId });

    public Task<HttpResponseMessage> UnbanTrackAsync(string hubId, string trackId) =>
        http.DeleteAsync($"/hubs/{hubId}/banned-tracks/{trackId}");

    // ── Voting ────────────────────────────────────────────────────────

    public Task<HttpResponseMessage> SuggestTrackAsync(string hubId, string trackId, TimeSpan duration) =>
        http.PostAsJsonAsync($"/hubs/{hubId}/suggestions", new { TrackId = trackId, Duration = duration });

    public Task<HttpResponseMessage> VoteAsync(string hubId, string userId, bool isUpVote) =>
        http.PostAsJsonAsync($"/hubs/{hubId}/vote", new { UserId = userId, IsUpVote = isUpVote });

    // ── Config ────────────────────────────────────────────────────────

    public Task<HttpResponseMessage> UpdateConfigAsync(string hubId, double fadeOutDurationSeconds) =>
        http.PatchAsJsonAsync($"/hubs/{hubId}/config", new { FadeOutDurationSeconds = fadeOutDurationSeconds });
}

// Input model for setting the track list
public record TrackInput(string Id, TimeSpan Duration);
