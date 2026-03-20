using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Musicratic.Core.Data;
using Musicratic.Core.Models;

namespace Musicratic.Core.Tests;

public class EfHubRepositoryTests
{
    private static MusicraticDbContext CreateContext(string dbName) =>
        new(new DbContextOptionsBuilder<MusicraticDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options);

    private static Hub MakeHub(string id = "h1", string ownerId = "owner") => new()
    {
        Id = id,
        Owner = new Owner { Id = ownerId },
        IsPublic = false,
        Config = new HubConfig(),
        Tracks = [],
        Queue = [],
        BannedTracks = [],
        SuggestedTracks = [],
        AttachedUsers = []
    };

    // ── AddAsync ──────────────────────────────────────────────────────

    [Fact]
    public async Task AddAsync_PersistsHubEntity()
    {
        var db = $"EfHub_{Guid.NewGuid()}";
        using var ctx = CreateContext(db);
        var repo = new EfHubRepository(ctx);
        var hub = MakeHub();

        await repo.AddAsync(hub);

        var entity = await ctx.Hubs.FindAsync("h1");
        entity.Should().NotBeNull();
        entity!.OwnerId.Should().Be("owner");
        entity.IsPublic.Should().BeFalse();
    }

    [Fact]
    public async Task GetByIdAsync_AfterAdd_ReturnsDomainHub()
    {
        var db = $"EfHub_{Guid.NewGuid()}";
        using var ctx = CreateContext(db);
        var repo = new EfHubRepository(ctx);
        var hub = MakeHub();
        hub.Config.FadeOutDuration = TimeSpan.FromSeconds(12);
        await repo.AddAsync(hub);

        var loaded = await repo.GetByIdAsync("h1");

        loaded.Should().NotBeNull();
        loaded!.Id.Should().Be("h1");
        loaded.Owner.Id.Should().Be("owner");
        loaded.Config.FadeOutDuration.TotalSeconds.Should().Be(12);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistent_ReturnsNull()
    {
        var db = $"EfHub_{Guid.NewGuid()}";
        using var ctx = CreateContext(db);
        var repo = new EfHubRepository(ctx);

        var result = await repo.GetByIdAsync("missing");

        result.Should().BeNull();
    }

    // ── SaveAsync — scalar fields ──────────────────────────────────────

    [Fact]
    public async Task SaveAsync_PersistsIsPublicChange()
    {
        var db = $"EfHub_{Guid.NewGuid()}";
        using var ctx = CreateContext(db);
        var repo = new EfHubRepository(ctx);
        var hub = MakeHub();
        await repo.AddAsync(hub);

        hub.IsPublic = true;
        await repo.SaveAsync(hub);

        var entity = await ctx.Hubs.FindAsync("h1");
        entity!.IsPublic.Should().BeTrue();
    }

    [Fact]
    public async Task SaveAsync_PersistsPlaybackState()
    {
        var db = $"EfHub_{Guid.NewGuid()}";
        using var ctx = CreateContext(db);
        var repo = new EfHubRepository(ctx);

        var track = new Track { Id = "t1", Duration = TimeSpan.FromMinutes(3) };
        var hub = MakeHub();
        hub.SetTrackList([track]);
        await repo.AddAsync(hub);

        // Directly set internal state (accessible via InternalsVisibleTo)
        hub.State = PlaybackState.Playing;
        hub.PlayingTrack = track;
        hub.PlayingTrackPosition = TimeSpan.FromSeconds(30);
        await repo.SaveAsync(hub);

        // Read back via a fresh repo on the same InMemory database
        using var ctx2 = CreateContext(db);
        var repo2 = new EfHubRepository(ctx2);
        var loaded = await repo2.GetByIdAsync("h1");

        loaded!.State.Should().Be(PlaybackState.Playing);
        loaded.PlayingTrack!.Id.Should().Be("t1");
        loaded.PlayingTrackPosition.TotalSeconds.Should().Be(30);
    }

    // ── SaveAsync — track collection ──────────────────────────────────

    [Fact]
    public async Task SaveAsync_PersistsTrackList()
    {
        var db = $"EfHub_{Guid.NewGuid()}";
        using var ctx = CreateContext(db);
        var repo = new EfHubRepository(ctx);
        var hub = MakeHub();
        await repo.AddAsync(hub);

        hub.SetTrackList([
            new Track { Id = "t1", Duration = TimeSpan.FromMinutes(3) },
            new Track { Id = "t2", Duration = TimeSpan.FromMinutes(4) },
        ]);
        await repo.SaveAsync(hub);

        using var ctx2 = CreateContext(db);
        var loaded = await new EfHubRepository(ctx2).GetByIdAsync("h1");

        loaded!.Tracks.Should().HaveCount(2);
        loaded.Tracks.Select(t => t.Id).Should().BeEquivalentTo(["t1", "t2"]);
    }

    [Fact]
    public async Task SaveAsync_ReplacesOldTracks()
    {
        var db = $"EfHub_{Guid.NewGuid()}";
        using var ctx = CreateContext(db);
        var repo = new EfHubRepository(ctx);
        var hub = MakeHub();
        hub.SetTrackList([new Track { Id = "old", Duration = TimeSpan.FromMinutes(2) }]);
        await repo.AddAsync(hub);
        await repo.SaveAsync(hub);

        hub.SetTrackList([new Track { Id = "new", Duration = TimeSpan.FromMinutes(3) }]);
        await repo.SaveAsync(hub);

        using var ctx2 = CreateContext(db);
        var loaded = await new EfHubRepository(ctx2).GetByIdAsync("h1");

        loaded!.Tracks.Should().ContainSingle(t => t.Id == "new");
        loaded.Tracks.Should().NotContain(t => t.Id == "old");
    }

    // ── SaveAsync — banned tracks & attached users ────────────────────

    [Fact]
    public async Task SaveAsync_PersistsBannedTracksAndAttachedUsers()
    {
        var db = $"EfHub_{Guid.NewGuid()}";
        using var ctx = CreateContext(db);
        var repo = new EfHubRepository(ctx);
        var hub = MakeHub();
        var track = new Track { Id = "t1", Duration = TimeSpan.FromMinutes(3) };
        hub.SetTrackList([track]);
        hub.BanTrack(track);
        hub.AttachUser(new User { Id = "u1" });
        await repo.AddAsync(hub);
        await repo.SaveAsync(hub);

        using var ctx2 = CreateContext(db);
        var loaded = await new EfHubRepository(ctx2).GetByIdAsync("h1");

        loaded!.BannedTracks.Should().ContainSingle(t => t.Id == "t1");
        loaded.AttachedUsers.Should().ContainSingle(u => u.Id == "u1");
    }
}
