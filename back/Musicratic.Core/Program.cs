using Musicratic.Core.Data;
using Musicratic.Core.Models;
using Musicratic.Core.Ports;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddDbContext<MusicraticDbContext>(opt =>
    opt.UseSqlite(builder.Configuration.GetConnectionString("Default") ?? "Data Source=musicratic.db"));
builder.Services.AddScoped<IUserRepository, EfUserRepository>();
builder.Services.AddScoped<IHubRepository, EfHubRepository>();
builder.Services.AddScoped<HubFactory>();

var app = builder.Build();

// ── Migrate on startup ───────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MusicraticDbContext>();
    if (db.Database.IsRelational())
        db.Database.Migrate();
    else
        db.Database.EnsureCreated();
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

// Unhandled exceptions → Problem Details (RFC 9457)
app.UseExceptionHandler(errApp =>
    errApp.Run(async ctx =>
    {
        var feature = ctx.Features.Get<IExceptionHandlerFeature>();
        ctx.Response.StatusCode = 500;
        await Results.Problem(
            detail: feature?.Error.Message,
            title: "An unexpected error occurred.",
            statusCode: 500)
            .ExecuteAsync(ctx);
    }));

// ── User CRUD ───────────────────────────────────────────────────────

app.MapPost("/users", async (UserRequest req, IUserRepository userRepo) =>
{
    if (await userRepo.GetByIdAsync(req.UserId) is not null)
        return Results.Problem(
            detail: $"User '{req.UserId}' already exists.",
            title: "Conflict",
            statusCode: 409);

    var user = new User { Id = req.UserId };
    await userRepo.AddAsync(user);
    return Results.Created($"/users/{user.Id}", new UserDto(user.Id));
})
.WithTags("Users")
.WithSummary("Create a user")
.WithDescription("Registers a new user in the system. Returns 409 if the user ID already exists.")
.Produces<UserDto>(StatusCodes.Status201Created)
.ProducesProblem(StatusCodes.Status409Conflict);

app.MapGet("/users", async (IUserRepository userRepo) =>
    Results.Ok((await userRepo.GetAllAsync()).Select(u => new UserDto(u.Id)).ToList()))
.WithTags("Users")
.WithSummary("List all users")
.WithDescription("Returns every registered user.")
.Produces<List<UserDto>>();

app.MapGet("/users/{userId}", async (string userId, IUserRepository userRepo) =>
{
    var user = await userRepo.GetByIdAsync(userId);
    return user is null
        ? Results.Problem(detail: $"User '{userId}' not found.", title: "Not Found", statusCode: 404)
        : Results.Ok(new UserDto(user.Id));
})
.WithTags("Users")
.WithSummary("Get a user")
.WithDescription("Returns a single user by ID.")
.Produces<UserDto>()
.ProducesProblem(StatusCodes.Status404NotFound);

app.MapDelete("/users/{userId}", async (string userId, IUserRepository userRepo) =>
{
    var user = await userRepo.GetByIdAsync(userId);
    if (user is null)
        return Results.Problem(detail: $"User '{userId}' not found.", title: "Not Found", statusCode: 404);

    await userRepo.RemoveAsync(user);
    return Results.NoContent();
})
.WithTags("Users")
.WithSummary("Delete a user")
.WithDescription("Permanently removes a user from the system.")
.Produces(StatusCodes.Status204NoContent)
.ProducesProblem(StatusCodes.Status404NotFound);

// ── Hub management ───────────────────────────────────────────────────

app.MapPost("/hubs", async (CreateHubRequest req, HubFactory hubFactory) =>
{
    var hub = await hubFactory.GetOrCreateAsync(req.HubId, new User { Id = req.OwnerId }, req.IsPublic);
    return Results.Ok(HubStatusDto.From(hub));
})
.WithTags("Hubs")
.WithSummary("Create or get a hub")
.WithDescription("Creates a new hub with the given ID and owner. If a hub with that ID already exists, returns the existing one.")
.Produces<HubStatusDto>();

app.MapGet("/hubs/{hubId}", async (string hubId, IHubRepository hubRepo) =>
{
    var hub = await hubRepo.GetByIdAsync(hubId);
    return hub is null
        ? Results.Problem(detail: $"Hub '{hubId}' not found.", title: "Not Found", statusCode: 404)
        : Results.Ok(HubStatusDto.From(hub));
})
.WithTags("Hubs")
.WithSummary("Get a hub")
.WithDescription("Returns the current state of the hub including playback, queue, voting, and user information.")
.Produces<HubStatusDto>()
.ProducesProblem(StatusCodes.Status404NotFound);

// ── Track list ───────────────────────────────────────────────────────

app.MapPut("/hubs/{hubId}/tracks", async (string hubId, SetTracksRequest req, IHubRepository hubRepo) =>
{
    var hub = await hubRepo.GetByIdAsync(hubId);
    if (hub is null)
        return Results.Problem(detail: $"Hub '{hubId}' not found.", title: "Not Found", statusCode: 404);

    var tracks = req.Tracks.Select(t => new Track { Id = t.Id, Duration = t.Duration }).ToList();
    hub.SetTrackList(tracks);
    await hubRepo.SaveAsync(hub);
    return Results.Ok(HubStatusDto.From(hub));
})
.WithTags("Tracks")
.WithSummary("Set the track list")
.WithDescription("Replaces the hub's track library and resets the playback queue to the new list.")
.Produces<HubStatusDto>()
.ProducesProblem(StatusCodes.Status404NotFound);

// ── Playback controls ────────────────────────────────────────────────

app.MapPost("/hubs/{hubId}/play", async (string hubId, IHubRepository hubRepo) =>
{
    var hub = await hubRepo.GetByIdAsync(hubId);
    if (hub is null)
        return Results.Problem(detail: $"Hub '{hubId}' not found.", title: "Not Found", statusCode: 404);

    hub.Play();
    await hubRepo.SaveAsync(hub);
    return Results.Ok(HubStatusDto.From(hub));
})
.WithTags("Playback")
.WithSummary("Play")
.WithDescription("Starts playback from the front of the queue. Does nothing if the queue is empty.")
.Produces<HubStatusDto>()
.ProducesProblem(StatusCodes.Status404NotFound);

app.MapPost("/hubs/{hubId}/stop", async (string hubId, IHubRepository hubRepo) =>
{
    var hub = await hubRepo.GetByIdAsync(hubId);
    if (hub is null)
        return Results.Problem(detail: $"Hub '{hubId}' not found.", title: "Not Found", statusCode: 404);

    hub.Stop();
    await hubRepo.SaveAsync(hub);
    return Results.Ok(HubStatusDto.From(hub));
})
.WithTags("Playback")
.WithSummary("Stop")
.WithDescription("Stops playback and resets the current track position.")
.Produces<HubStatusDto>()
.ProducesProblem(StatusCodes.Status404NotFound);

app.MapPost("/hubs/{hubId}/next", async (string hubId, IHubRepository hubRepo) =>
{
    var hub = await hubRepo.GetByIdAsync(hubId);
    if (hub is null)
        return Results.Problem(detail: $"Hub '{hubId}' not found.", title: "Not Found", statusCode: 404);

    hub.Next();
    await hubRepo.SaveAsync(hub);
    return Results.Ok(HubStatusDto.From(hub));
})
.WithTags("Playback")
.WithSummary("Skip to next track")
.WithDescription("Removes the current track from the queue and starts playing the next one.")
.Produces<HubStatusDto>()
.ProducesProblem(StatusCodes.Status404NotFound);

app.MapPost("/hubs/{hubId}/shuffle", async (string hubId, IHubRepository hubRepo) =>
{
    var hub = await hubRepo.GetByIdAsync(hubId);
    if (hub is null)
        return Results.Problem(detail: $"Hub '{hubId}' not found.", title: "Not Found", statusCode: 404);

    hub.Shuffle();
    await hubRepo.SaveAsync(hub);
    return Results.Ok(HubStatusDto.From(hub));
})
.WithTags("Playback")
.WithSummary("Shuffle queue")
.WithDescription("Randomly reorders the tracks currently in the playback queue. Does not affect the track library.")
.Produces<HubStatusDto>()
.ProducesProblem(StatusCodes.Status404NotFound);

// ── Hub users ────────────────────────────────────────────────────────

app.MapPost("/hubs/{hubId}/users", async (string hubId, UserRequest req, IHubRepository hubRepo, IUserRepository userRepo) =>
{
    var hub = await hubRepo.GetByIdAsync(hubId);
    if (hub is null)
        return Results.Problem(detail: $"Hub '{hubId}' not found.", title: "Not Found", statusCode: 404);

    var user = await userRepo.GetByIdAsync(req.UserId) ?? new User { Id = req.UserId };
    if (await userRepo.GetByIdAsync(req.UserId) is null)
        await userRepo.AddAsync(user);

    hub.AttachUser(user);
    await hubRepo.SaveAsync(hub);
    return Results.Ok(HubStatusDto.From(hub));
})
.WithTags("Hub Users")
.WithSummary("Attach a user to a hub")
.WithDescription("Registers the user in the system (if not already present) and attaches them to the hub so they can vote.")
.Produces<HubStatusDto>()
.ProducesProblem(StatusCodes.Status404NotFound);

app.MapDelete("/hubs/{hubId}/users/{userId}", async (string hubId, string userId, IHubRepository hubRepo) =>
{
    var hub = await hubRepo.GetByIdAsync(hubId);
    if (hub is null)
        return Results.Problem(detail: $"Hub '{hubId}' not found.", title: "Not Found", statusCode: 404);

    var user = hub.AttachedUsers.FirstOrDefault(u => u.Id == userId);
    if (user is null)
        return Results.Problem(
            detail: $"User '{userId}' is not attached to hub '{hubId}'.",
            title: "Not Found",
            statusCode: 404);

    hub.DetachUser(user);
    await hubRepo.SaveAsync(hub);
    return Results.Ok(HubStatusDto.From(hub));
})
.WithTags("Hub Users")
.WithSummary("Detach a user from a hub")
.WithDescription("Removes the user from the hub's attendee list. The user still exists in the system.")
.Produces<HubStatusDto>()
.ProducesProblem(StatusCodes.Status404NotFound);

// ── Ban list ─────────────────────────────────────────────────────────

app.MapPost("/hubs/{hubId}/banned-tracks", async (string hubId, TrackIdRequest req, IHubRepository hubRepo) =>
{
    var hub = await hubRepo.GetByIdAsync(hubId);
    if (hub is null)
        return Results.Problem(detail: $"Hub '{hubId}' not found.", title: "Not Found", statusCode: 404);

    var track = hub.Tracks.FirstOrDefault(t => t.Id == req.TrackId)
             ?? hub.Queue.FirstOrDefault(t => t.Id == req.TrackId);
    if (track is null)
        return Results.Problem(
            detail: $"Track '{req.TrackId}' was not found in hub '{hubId}'.",
            title: "Not Found",
            statusCode: 404);

    hub.BanTrack(track);
    await hubRepo.SaveAsync(hub);
    return Results.Ok(HubStatusDto.From(hub));
})
.WithTags("Ban List")
.WithSummary("Ban a track")
.WithDescription("Adds a track to the hub's ban list. Banned tracks cannot be suggested by users.")
.Produces<HubStatusDto>()
.ProducesProblem(StatusCodes.Status404NotFound);

app.MapDelete("/hubs/{hubId}/banned-tracks/{trackId}", async (string hubId, string trackId, IHubRepository hubRepo) =>
{
    var hub = await hubRepo.GetByIdAsync(hubId);
    if (hub is null)
        return Results.Problem(detail: $"Hub '{hubId}' not found.", title: "Not Found", statusCode: 404);

    var track = hub.BannedTracks.FirstOrDefault(t => t.Id == trackId);
    if (track is null)
        return Results.Problem(
            detail: $"Track '{trackId}' is not in the ban list for hub '{hubId}'.",
            title: "Not Found",
            statusCode: 404);

    hub.UnbanTrack(track);
    await hubRepo.SaveAsync(hub);
    return Results.Ok(HubStatusDto.From(hub));
})
.WithTags("Ban List")
.WithSummary("Unban a track")
.WithDescription("Removes a track from the hub's ban list, allowing it to be suggested again.")
.Produces<HubStatusDto>()
.ProducesProblem(StatusCodes.Status404NotFound);

// ── Suggestions & voting ─────────────────────────────────────────────

app.MapPost("/hubs/{hubId}/suggestions", async (string hubId, SuggestTrackRequest req, IHubRepository hubRepo) =>
{
    var hub = await hubRepo.GetByIdAsync(hubId);
    if (hub is null)
        return Results.Problem(detail: $"Hub '{hubId}' not found.", title: "Not Found", statusCode: 404);

    var track = new Track { Id = req.TrackId, Duration = req.Duration };
    try
    {
        hub.SuggestTrack(track);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(detail: ex.Message, title: "Suggestion Not Allowed", statusCode: 422);
    }

    return Results.Ok(HubStatusDto.From(hub));
})
.WithTags("Voting")
.WithSummary("Suggest a track")
.WithDescription("""
    Adds a track to the suggestion queue and triggers a public vote.

    **Voting rules:**
    - Voting lasts up to 60 s split into two phases:
      - **Phase A** (45 s) — overlaps with the tail of the currently playing song.
      - **Phase B** (15 s) — overlaps with the beginning of the suggested song.
    - A track is rejected if ≥ 65 % of votes are downvotes.
    - 0 votes = passed.
    - The hub owner's downvote is an immediate veto.
    - Phase A rejection: the suggested song never plays.
    - Phase B rejection: the suggested song fades out and the normal queue resumes.

    **Constraints:** track duration ≤ 6 min; track must not be banned; the queue must contain at least one track ≥ 45 s.
    """)
.Produces<HubStatusDto>()
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status422UnprocessableEntity);

app.MapPost("/hubs/{hubId}/vote", async (string hubId, VoteRequest req, IHubRepository hubRepo) =>
{
    var hub = await hubRepo.GetByIdAsync(hubId);
    if (hub is null)
        return Results.Problem(detail: $"Hub '{hubId}' not found.", title: "Not Found", statusCode: 404);

    var user = hub.AttachedUsers.FirstOrDefault(u => u.Id == req.UserId);
    if (user is null)
        return Results.Problem(
            detail: $"User '{req.UserId}' is not attached to hub '{hubId}'.",
            title: "Unattached User",
            statusCode: 422);

    try
    {
        hub.CastVote(req.IsUpVote, user);
    }
    catch (InvalidOperationException ex)
    {
        return Results.Problem(detail: ex.Message, title: "Vote Not Allowed", statusCode: 422);
    }

    return Results.Ok(HubStatusDto.From(hub));
})
.WithTags("Voting")
.WithSummary("Cast a vote")
.WithDescription("Casts an upvote or downvote on the currently active suggestion. The user must be attached to the hub. The owner casting a downvote immediately vetoes the suggestion.")
.Produces<HubStatusDto>()
.ProducesProblem(StatusCodes.Status404NotFound)
.ProducesProblem(StatusCodes.Status422UnprocessableEntity);

// ── Config ───────────────────────────────────────────────────────────

app.MapPatch("/hubs/{hubId}/config", async (string hubId, UpdateConfigRequest req, IHubRepository hubRepo) =>
{
    var hub = await hubRepo.GetByIdAsync(hubId);
    if (hub is null)
        return Results.Problem(detail: $"Hub '{hubId}' not found.", title: "Not Found", statusCode: 404);

    if (req.FadeOutDurationSeconds.HasValue)
        hub.Config.FadeOutDuration = TimeSpan.FromSeconds(req.FadeOutDurationSeconds.Value);

    await hubRepo.SaveAsync(hub);
    return Results.Ok(HubStatusDto.From(hub));
})
.WithTags("Config")
.WithSummary("Update hub config")
.WithDescription("Updates configurable hub settings. `fadeOutDurationSeconds` is clamped to the range [5, 15] (default 10).")
.Produces<HubStatusDto>()
.ProducesProblem(StatusCodes.Status404NotFound);

app.Run();

// ── Request / Response DTOs ──────────────────────────────────────────

record CreateHubRequest(string HubId, string OwnerId, bool IsPublic = false);
record SetTracksRequest(List<TrackDto> Tracks);
record TrackDto(string Id, TimeSpan Duration);
record UserRequest(string UserId);
record TrackIdRequest(string TrackId);
record SuggestTrackRequest(string TrackId, TimeSpan Duration);
record VoteRequest(string UserId, bool IsUpVote);
record UpdateConfigRequest(double? FadeOutDurationSeconds);
record UserDto(string Id);

record HubStatusDto(
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
    double FadeOutDurationSeconds)
{
    public static HubStatusDto From(Hub hub) => new(
        hub.Id,
        hub.Owner.Id,
        hub.IsPublic,
        hub.State.ToString(),
        hub.PlayingTrack?.Id,
        hub.PlayingTrackPosition.TotalSeconds,
        hub.IsSuggestedTrackPlaying,
        hub.CanSuggest,
        hub.CurrentVoting is not null
            ? new VotingDto(
                hub.CurrentVoting.Track.Id,
                hub.CurrentVoting.UpVotes,
                hub.CurrentVoting.DownVotes,
                hub.CurrentVoting.TimeLeft.TotalSeconds,
                hub.CurrentVoting.Phase.ToString(),
                hub.CurrentVoting.IsActive)
            : null,
        hub.Queue.Select(t => t.Id).ToList(),
        hub.SuggestedTracks.Select(t => t.Id).ToList(),
        hub.AttachedUsers.Select(u => u.Id).ToList(),
        hub.BannedTracks.Select(t => t.Id).ToList(),
        hub.Config.FadeOutDuration.TotalSeconds);
}

record VotingDto(
    string TrackId,
    int UpVotes,
    int DownVotes,
    double TimeLeftSeconds,
    string Phase,
    bool IsActive);

// Required for WebApplicationFactory<Program> in integration tests
public partial class Program { }
