using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Core.Data;

namespace Musicratic.Core.Tests;

/// <summary>
/// Integration tests for the Minimal API endpoints.
/// StateStore is reset before each test class instantiation (xUnit creates one instance per test).
/// </summary>
[Collection("MusicraticSuite")]
public class ApiTests : IDisposable
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;
    private static readonly JsonSerializerOptions Json =
        new(JsonSerializerDefaults.Web);

    public ApiTests()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove every service registered by the SQLite AddDbContext call
                var toRemove = services
                    .Where(d => d.ServiceType.FullName?.Contains("EntityFrameworkCore") == true
                             || d.ImplementationType?.FullName?.Contains("EntityFrameworkCore") == true
                             || d.ServiceType == typeof(MusicraticDbContext)
                             || d.ServiceType == typeof(DbContextOptions<MusicraticDbContext>))
                    .ToList();
                foreach (var d in toRemove)
                    services.Remove(d);

                // Capture the database name BEFORE the lambda — the options action is called
                // once per scope, so values evaluated inside it differ across scopes.
                var dbName = $"TestDb_{Guid.NewGuid()}";
                services.AddDbContext<MusicraticDbContext>(opt =>
                    opt.UseInMemoryDatabase(dbName));
            });
        });
        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // ── Helpers ───────────────────────────────────────────────────────

    private async Task<JsonElement> PostJsonAsync(string url, object body)
    {
        var response = await _client.PostAsJsonAsync(url, body);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>(Json);
    }

    private async Task<JsonElement> GetJsonAsync(string url)
    {
        var response = await _client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<JsonElement>(Json);
    }

    private async Task CreateHubAsync(string hubId = "h1", string ownerId = "owner")
    {
        var response = await _client.PostAsJsonAsync("/hubs", new { hubId, ownerId, isPublic = false });
        response.EnsureSuccessStatusCode();
    }

    // ── POST /hubs ────────────────────────────────────────────────────

    [Fact]
    public async Task PostHub_CreatesHub_Returns200WithId()
    {
        var response = await _client.PostAsJsonAsync("/hubs",
            new { hubId = "h1", ownerId = "owner", isPublic = false });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("id").GetString().Should().Be("h1");
        body.GetProperty("ownerId").GetString().Should().Be("owner");
    }

    [Fact]
    public async Task PostHub_CalledTwice_ReturnsSameHub()
    {
        await _client.PostAsJsonAsync("/hubs", new { hubId = "h1", ownerId = "owner" });
        var response = await _client.PostAsJsonAsync("/hubs", new { hubId = "h1", ownerId = "other" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("ownerId").GetString().Should().Be("owner"); // original owner preserved
    }

    // ── GET /hubs/{hubId} ─────────────────────────────────────────────

    [Fact]
    public async Task GetHub_ExistingHub_Returns200()
    {
        await CreateHubAsync();

        var response = await _client.GetAsync("/hubs/h1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetHub_NonExistentHub_Returns404WithProblemDetails()
    {
        var response = await _client.GetAsync("/hubs/missing");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("title").GetString().Should().Be("Not Found");
        body.GetProperty("status").GetInt32().Should().Be(404);
    }

    // ── PUT /hubs/{hubId}/tracks ──────────────────────────────────────

    [Fact]
    public async Task PutTracks_SetsQueueAndTracks()
    {
        await CreateHubAsync();

        var response = await _client.PutAsJsonAsync("/hubs/h1/tracks", new
        {
            tracks = new[]
            {
                new { id = "t1", duration = "00:03:00" },
                new { id = "t2", duration = "00:02:00" }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("queueTrackIds").GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task PutTracks_NonExistentHub_Returns404()
    {
        var response = await _client.PutAsJsonAsync("/hubs/missing/tracks",
            new { tracks = Array.Empty<object>() });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /hubs/{hubId}/play ───────────────────────────────────────

    [Fact]
    public async Task PostPlay_StartsPlayback_Returns200()
    {
        await CreateHubAsync();
        await _client.PutAsJsonAsync("/hubs/h1/tracks", new
        {
            tracks = new[] { new { id = "t1", duration = "00:03:00" } }
        });

        var response = await _client.PostAsync("/hubs/h1/play", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("playbackState").GetString().Should().Be("Playing");
    }

    [Fact]
    public async Task PostPlay_NonExistentHub_Returns404()
    {
        var response = await _client.PostAsync("/hubs/missing/play", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /hubs/{hubId}/stop ───────────────────────────────────────

    [Fact]
    public async Task PostStop_StopsPlayback_Returns200()
    {
        await CreateHubAsync();
        var response = await _client.PostAsync("/hubs/h1/stop", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("playbackState").GetString().Should().Be("Stopped");
    }

    [Fact]
    public async Task PostStop_NonExistentHub_Returns404()
    {
        var response = await _client.PostAsync("/hubs/missing/stop", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /hubs/{hubId}/next ───────────────────────────────────────

    [Fact]
    public async Task PostNext_Returns200()
    {
        await CreateHubAsync();
        var response = await _client.PostAsync("/hubs/h1/next", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostNext_NonExistentHub_Returns404()
    {
        var response = await _client.PostAsync("/hubs/missing/next", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /hubs/{hubId}/shuffle ────────────────────────────────────

    [Fact]
    public async Task PostShuffle_Returns200()
    {
        await CreateHubAsync();
        var response = await _client.PostAsync("/hubs/h1/shuffle", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task PostShuffle_NonExistentHub_Returns404()
    {
        var response = await _client.PostAsync("/hubs/missing/shuffle", null);

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── POST /hubs/{hubId}/users ──────────────────────────────────────

    [Fact]
    public async Task PostUser_AttachesUser_Returns200()
    {
        await CreateHubAsync();

        var response = await _client.PostAsJsonAsync("/hubs/h1/users",
            new { userId = "u1" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("attachedUserIds").EnumerateArray()
            .Select(e => e.GetString()).Should().Contain("u1");
    }

    [Fact]
    public async Task PostUser_NonExistentHub_Returns404()
    {
        var response = await _client.PostAsJsonAsync("/hubs/missing/users",
            new { userId = "u1" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── DELETE /hubs/{hubId}/users/{userId} ───────────────────────────

    [Fact]
    public async Task DeleteUser_DetachesUser_Returns200()
    {
        await CreateHubAsync();
        await _client.PostAsJsonAsync("/hubs/h1/users", new { userId = "u1" });

        var response = await _client.DeleteAsync("/hubs/h1/users/u1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("attachedUserIds").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task DeleteUser_NonExistentHub_Returns404()
    {
        var response = await _client.DeleteAsync("/hubs/missing/users/u1");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_UserNotAttached_Returns404WithProblemDetails()
    {
        await CreateHubAsync();

        var response = await _client.DeleteAsync("/hubs/h1/users/u999");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    // ── POST /hubs/{hubId}/banned-tracks ─────────────────────────────

    [Fact]
    public async Task PostBannedTrack_BansTrack_Returns200()
    {
        await CreateHubAsync();
        await _client.PutAsJsonAsync("/hubs/h1/tracks", new
        {
            tracks = new[] { new { id = "t1", duration = "00:03:00" } }
        });

        var response = await _client.PostAsJsonAsync("/hubs/h1/banned-tracks",
            new { trackId = "t1" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("bannedTrackIds").EnumerateArray()
            .Select(e => e.GetString()).Should().Contain("t1");
    }

    [Fact]
    public async Task PostBannedTrack_NonExistentHub_Returns404()
    {
        var response = await _client.PostAsJsonAsync("/hubs/missing/banned-tracks",
            new { trackId = "t1" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostBannedTrack_TrackNotInHub_Returns404WithProblemDetails()
    {
        await CreateHubAsync();

        var response = await _client.PostAsJsonAsync("/hubs/h1/banned-tracks",
            new { trackId = "unknown" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    // ── DELETE /hubs/{hubId}/banned-tracks/{trackId} ──────────────────

    [Fact]
    public async Task DeleteBannedTrack_UnbansTrack_Returns200()
    {
        await CreateHubAsync();
        await _client.PutAsJsonAsync("/hubs/h1/tracks", new
        {
            tracks = new[] { new { id = "t1", duration = "00:03:00" } }
        });
        await _client.PostAsJsonAsync("/hubs/h1/banned-tracks", new { trackId = "t1" });

        var response = await _client.DeleteAsync("/hubs/h1/banned-tracks/t1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("bannedTrackIds").GetArrayLength().Should().Be(0);
    }

    [Fact]
    public async Task DeleteBannedTrack_TrackNotBanned_Returns404WithProblemDetails()
    {
        await CreateHubAsync();

        var response = await _client.DeleteAsync("/hubs/h1/banned-tracks/notbanned");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    // ── POST /hubs/{hubId}/suggestions ────────────────────────────────

    [Fact]
    public async Task PostSuggestion_ValidTrack_Returns200()
    {
        await CreateHubAsync();
        await _client.PutAsJsonAsync("/hubs/h1/tracks", new
        {
            tracks = new[] { new { id = "t1", duration = "00:01:30" } } // 90 s
        });

        var response = await _client.PostAsJsonAsync("/hubs/h1/suggestions",
            new { trackId = "s1", duration = "00:02:00" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("suggestedTrackIds").EnumerateArray()
            .Select(e => e.GetString()).Should().Contain("s1");
    }

    [Fact]
    public async Task PostSuggestion_NonExistentHub_Returns404()
    {
        var response = await _client.PostAsJsonAsync("/hubs/missing/suggestions",
            new { trackId = "s1", duration = "00:02:00" });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PostSuggestion_CanSuggestFalse_Returns422WithProblemDetails()
    {
        await CreateHubAsync(); // no tracks → CanSuggest = false

        var response = await _client.PostAsJsonAsync("/hubs/h1/suggestions",
            new { trackId = "s1", duration = "00:02:00" });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("title").GetString().Should().Be("Suggestion Not Allowed");
    }

    // ── POST /hubs/{hubId}/vote ───────────────────────────────────────

    [Fact]
    public async Task PostVote_NoActiveVoting_Returns422WithProblemDetails()
    {
        await CreateHubAsync();
        await _client.PostAsJsonAsync("/hubs/h1/users", new { userId = "u1" });

        var response = await _client.PostAsJsonAsync("/hubs/h1/vote",
            new { userId = "u1", isUpVote = true });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    [Fact]
    public async Task PostVote_UserNotAttached_Returns422WithProblemDetails()
    {
        await CreateHubAsync();

        var response = await _client.PostAsJsonAsync("/hubs/h1/vote",
            new { userId = "unattached", isUpVote = true });

        response.StatusCode.Should().Be(HttpStatusCode.UnprocessableEntity);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("title").GetString().Should().Be("Unattached User");
    }

    [Fact]
    public async Task PostVote_NonExistentHub_Returns404()
    {
        var response = await _client.PostAsJsonAsync("/hubs/missing/vote",
            new { userId = "u1", isUpVote = true });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    // ── PATCH /hubs/{hubId}/config ────────────────────────────────────

    [Fact]
    public async Task PatchConfig_UpdatesFadeOutDuration_Returns200()
    {
        await CreateHubAsync();

        var response = await _client.PatchAsJsonAsync("/hubs/h1/config",
            new { fadeOutDurationSeconds = 8.0 });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("fadeOutDurationSeconds").GetDouble().Should().Be(8.0);
    }

    [Fact]
    public async Task PatchConfig_NonExistentHub_Returns404()
    {
        var response = await _client.PatchAsJsonAsync("/hubs/missing/config",
            new { fadeOutDurationSeconds = 8.0 });

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task PatchConfig_NullFadeOut_NoChangeToConfig()
    {
        await CreateHubAsync();

        var response = await _client.PatchAsJsonAsync("/hubs/h1/config",
            new { fadeOutDurationSeconds = (double?)null });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("fadeOutDurationSeconds").GetDouble().Should().Be(10.0); // default
    }

    // ── POST /users ───────────────────────────────────────────────────

    [Fact]
    public async Task PostUser_CreatesUser_Returns201()
    {
        var response = await _client.PostAsJsonAsync("/users", new { userId = "u1" });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location!.ToString().Should().Be("/users/u1");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("id").GetString().Should().Be("u1");
    }

    [Fact]
    public async Task PostUser_DuplicateId_Returns409WithProblemDetails()
    {
        await _client.PostAsJsonAsync("/users", new { userId = "u1" });
        var response = await _client.PostAsJsonAsync("/users", new { userId = "u1" });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("title").GetString().Should().Be("Conflict");
    }

    // ── GET /users ─────────────────────────────────────────────────────

    [Fact]
    public async Task GetUsers_ReturnsAllUsers()
    {
        await _client.PostAsJsonAsync("/users", new { userId = "u1" });
        await _client.PostAsJsonAsync("/users", new { userId = "u2" });

        var response = await _client.GetAsync("/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetArrayLength().Should().Be(2);
    }

    [Fact]
    public async Task GetUsers_Empty_ReturnsEmptyArray()
    {
        var response = await _client.GetAsync("/users");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetArrayLength().Should().Be(0);
    }

    // ── GET /users/{userId} ───────────────────────────────────────────────

    [Fact]
    public async Task GetUser_ExistingUser_Returns200()
    {
        await _client.PostAsJsonAsync("/users", new { userId = "u1" });

        var response = await _client.GetAsync("/users/u1");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<JsonElement>(Json);
        body.GetProperty("id").GetString().Should().Be("u1");
    }

    [Fact]
    public async Task GetUser_NonExistentUser_Returns404WithProblemDetails()
    {
        var response = await _client.GetAsync("/users/missing");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }

    // ── DELETE /users/{userId} ────────────────────────────────────────────

    [Fact]
    public async Task DeleteUser_ExistingUser_Returns204()
    {
        await _client.PostAsJsonAsync("/users", new { userId = "u1" });

        var response = await _client.DeleteAsync("/users/u1");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        var verify = await _client.GetAsync("/users/u1");
        verify.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteUser_NonExistentUser_Returns404WithProblemDetails()
    {
        var response = await _client.DeleteAsync("/users/missing");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/problem+json");
    }
}
