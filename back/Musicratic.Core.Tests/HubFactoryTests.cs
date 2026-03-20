using FluentAssertions;
using Musicratic.Core.Models;
using Musicratic.Core.Ports;

namespace Musicratic.Core.Tests;

[Collection("MusicraticSuite")]
public class HubFactoryTests
{
    // ── Stub repository ───────────────────────────────────────────────

    private class StubHubRepository : IHubRepository
    {
        private readonly List<Hub> _hubs = [];

        public Task<Hub?> GetByIdAsync(string id) =>
            Task.FromResult(_hubs.FirstOrDefault(h => h.Id == id));

        public Task AddAsync(Hub hub)
        {
            _hubs.Add(hub);
            return Task.CompletedTask;
        }

        public Task SaveAsync(Hub hub) => Task.CompletedTask;
    }

    // ── Tests ─────────────────────────────────────────────────────────

    [Fact]
    public async Task GetOrCreate_NewId_CreatesAndReturnsHub()
    {
        var factory = new HubFactory(new StubHubRepository());
        var owner = new User { Id = "owner" };

        var hub = await factory.GetOrCreateAsync("h1", owner, isPublic: true);

        hub.Should().NotBeNull();
        hub.Id.Should().Be("h1");
        hub.Owner.Id.Should().Be("owner");
        hub.IsPublic.Should().BeTrue();
    }

    [Fact]
    public async Task GetOrCreate_ExistingId_ReturnsSameInstance()
    {
        var repo = new StubHubRepository();
        var factory = new HubFactory(repo);
        var owner = new User { Id = "owner" };

        var first = await factory.GetOrCreateAsync("h1", owner);
        var second = await factory.GetOrCreateAsync("h1", new User { Id = "other" });

        second.Should().BeSameAs(first);
    }

    [Fact]
    public async Task GetOrCreate_RegistersHubInRepository()
    {
        var repo = new StubHubRepository();
        var factory = new HubFactory(repo);

        await factory.GetOrCreateAsync("h1", new User { Id = "owner" });

        (await repo.GetByIdAsync("h1")).Should().NotBeNull();
    }

    [Fact]
    public async Task GetOrCreate_NewHub_HasOwnerAsOwnerType()
    {
        var factory = new HubFactory(new StubHubRepository());

        var hub = await factory.GetOrCreateAsync("h1", new User { Id = "owner" });

        hub.Owner.Should().BeOfType<Owner>();
    }

    [Fact]
    public async Task GetOrCreate_NewHub_HasEmptyCollections()
    {
        var factory = new HubFactory(new StubHubRepository());

        var hub = await factory.GetOrCreateAsync("h1", new User { Id = "owner" });

        hub.Tracks.Should().BeEmpty();
        hub.Queue.Should().BeEmpty();
        hub.BannedTracks.Should().BeEmpty();
        hub.SuggestedTracks.Should().BeEmpty();
        hub.AttachedUsers.Should().BeEmpty();
    }
}
