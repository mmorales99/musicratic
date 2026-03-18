using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Economy.Domain.Entities;
using Musicratic.Economy.Infrastructure.Persistence;
using Musicratic.Hub.Infrastructure.Persistence;
using Musicratic.Playback.Domain.Entities;
using Musicratic.Playback.Domain.Enums;
using Musicratic.Playback.Infrastructure.Persistence;
using Musicratic.TestUtilities.Auth;
using Musicratic.TestUtilities.Builders;
using Musicratic.TestUtilities.Seeders;
using Musicratic.Voting.Application.Services;
using Musicratic.Voting.Infrastructure.Persistence;

namespace Musicratic.Integration.Tests;

/// <summary>
/// TEST-006: Integration test for the full propose → vote → skip → refund flow.
/// Tests cross-module interaction between Playback, Voting, and Economy modules.
/// </summary>
public class ProposalVoteSkipFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly CustomWebApplicationFactory _factory;

    public ProposalVoteSkipFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProposeTrackPaid_WithSufficientBalance_CreatesQueueEntry()
    {
        // Arrange
        var proposerId = Guid.NewGuid();
        var hub = await SeedActiveHub();
        var track = await SeedTrack();
        await SeedWalletWithBalance(proposerId, hub.TenantId, 100);

        var client = CreateAuthenticatedClient(userId: proposerId.ToString());

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/hubs/{hub.Id}/queue/propose-paid",
            new { TrackId = track.Id, ProposerId = proposerId, CoinAmount = 10 });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var proposal = await DeserializeResponse<ProposalResponse>(response);
        proposal.QueueEntryId.Should().NotBeEmpty();
        proposal.TrackId.Should().Be(track.Id);
        proposal.Title.Should().Be(track.Title);
    }

    [Fact]
    public async Task CastVote_OnPlayingTrack_ReturnsVoteCreated()
    {
        // Arrange
        var voterId = Guid.NewGuid();
        var hub = await SeedActiveHub();
        var track = await SeedTrack();
        var entry = await SeedPlayingQueueEntry(hub, track);
        OpenVotingWindow(hub.Id, entry.Id);

        var client = CreateAuthenticatedClient(userId: voterId.ToString());

        // Act
        var response = await client.PostAsJsonAsync(
            $"/api/hubs/{hub.Id}/queue/{entry.Id}/vote",
            new { Value = "Down" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetTally_AfterMultipleVotes_ReturnsCorrectCounts()
    {
        // Arrange
        var hub = await SeedActiveHub();
        var track = await SeedTrack();
        var entry = await SeedPlayingQueueEntry(hub, track);
        OpenVotingWindow(hub.Id, entry.Id);

        // Cast 2 downvotes and 1 upvote
        var voter1 = Guid.NewGuid();
        var voter2 = Guid.NewGuid();
        var voter3 = Guid.NewGuid();

        await CastVoteViaApi(hub.Id, entry.Id, voter1, "Down");
        await CastVoteViaApi(hub.Id, entry.Id, voter2, "Down");
        await CastVoteViaApi(hub.Id, entry.Id, voter3, "Up");

        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync($"/api/hubs/{hub.Id}/queue/{entry.Id}/tally");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var tally = await DeserializeResponse<VoteTallyResponse>(response);
        tally.Upvotes.Should().Be(1);
        tally.Downvotes.Should().Be(2);
        tally.Total.Should().Be(3);
    }

    [Fact]
    public async Task SkipTrack_WhenPlaying_ReturnsNoContent()
    {
        // Arrange
        var hub = await SeedActiveHub();
        var track = await SeedTrack();
        await SeedPlayingQueueEntry(hub, track);

        var client = CreateAuthenticatedClient(role: "hub_manager");

        // Act
        var response = await client.PostAsync($"/api/hubs/{hub.Id}/queue/skip", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task FullFlow_Propose_Vote_Skip_VerifyState()
    {
        // Arrange — create hub, track, wallet, and propose a paid track
        var proposerId = Guid.NewGuid();
        var hub = await SeedActiveHub();
        var track = await SeedTrack("Skip Target", "Skip Artist");
        await SeedWalletWithBalance(proposerId, hub.TenantId, 100);

        var proposerClient = CreateAuthenticatedClient(userId: proposerId.ToString());

        // Step 1: Propose a paid track → goes directly to queue
        var proposeResponse = await proposerClient.PostAsJsonAsync(
            $"/api/hubs/{hub.Id}/queue/propose-paid",
            new { TrackId = track.Id, ProposerId = proposerId, CoinAmount = 20 });
        proposeResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var proposal = await DeserializeResponse<ProposalResponse>(proposeResponse);
        var entryId = proposal.QueueEntryId;

        // Step 2: Start playback — the queued entry starts playing
        var managerClient = CreateAuthenticatedClient(role: "hub_manager");
        var startResponse = await managerClient.PostAsync($"/api/hubs/{hub.Id}/playback/start", null);
        startResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 3: Verify now-playing
        var nowPlayingResponse = await managerClient.GetAsync($"/api/hubs/{hub.Id}/now-playing");
        nowPlayingResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var nowPlaying = await DeserializeResponse<NowPlayingResponse>(nowPlayingResponse);
        nowPlaying.TrackId.Should().Be(track.Id);

        // Step 4: Open voting window and cast 3 downvotes (triggers 100% > 65% skip rule)
        OpenVotingWindow(hub.Id, entryId);

        var voter1Id = Guid.NewGuid();
        var voter2Id = Guid.NewGuid();
        var voter3Id = Guid.NewGuid();

        await CastVoteViaApi(hub.Id, entryId, voter1Id, "Down");
        await CastVoteViaApi(hub.Id, entryId, voter2Id, "Down");
        await CastVoteViaApi(hub.Id, entryId, voter3Id, "Down");

        // Step 5: Verify tally shows 100% downvotes
        var tallyResponse = await managerClient.GetAsync($"/api/hubs/{hub.Id}/queue/{entryId}/tally");
        tallyResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tally = await DeserializeResponse<VoteTallyResponse>(tallyResponse);
        tally.Downvotes.Should().Be(3);
        tally.DownvotePercentage.Should().Be(100.0);

        // Step 6: Skip the track (simulates Dapr-triggered skip in production)
        var skipResponse = await managerClient.PostAsync($"/api/hubs/{hub.Id}/queue/skip", null);
        skipResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Step 7: Verify queue is empty (no more tracks playing)
        var afterSkipNowPlaying = await managerClient.GetAsync($"/api/hubs/{hub.Id}/now-playing");
        afterSkipNowPlaying.StatusCode.Should().Be(HttpStatusCode.NotFound);

        // Step 8: Verify wallet was debited correctly (original 100 - 20 = 80)
        var walletBalance = await GetWalletBalance(proposerId, hub.TenantId);
        walletBalance.Should().Be(80m);
    }

    [Fact]
    public async Task DuplicateVote_SameUserSameEntry_ReturnsConflict()
    {
        // Arrange
        var voterId = Guid.NewGuid();
        var hub = await SeedActiveHub();
        var track = await SeedTrack();
        var entry = await SeedPlayingQueueEntry(hub, track);
        OpenVotingWindow(hub.Id, entry.Id);

        // First vote succeeds
        await CastVoteViaApi(hub.Id, entry.Id, voterId, "Down");

        // Act — second vote from same user
        var client = CreateAuthenticatedClient(userId: voterId.ToString());
        var response = await client.PostAsJsonAsync(
            $"/api/hubs/{hub.Id}/queue/{entry.Id}/vote",
            new { Value = "Up" });

        // Assert — duplicate vote rejected
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    // --- Helpers ---

    private HttpClient CreateAuthenticatedClient(
        string? userId = null,
        string role = "user")
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(
            TestAuthHandler.UserIdHeader,
            userId ?? Guid.NewGuid().ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    private async Task CastVoteViaApi(Guid hubId, Guid entryId, Guid voterId, string value)
    {
        var client = CreateAuthenticatedClient(userId: voterId.ToString());
        var response = await client.PostAsJsonAsync(
            $"/api/hubs/{hubId}/queue/{entryId}/vote",
            new { Value = value });
        response.StatusCode.Should().Be(HttpStatusCode.Created,
            $"Vote from {voterId} should succeed");
    }

    private async Task<Musicratic.Hub.Domain.Entities.Hub> SeedActiveHub(string name = "Test Hub")
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HubDbContext>();

        var hub = await TestDataSeeder.SeedHub(dbContext, b => b.WithName(name));
        hub.Activate();
        dbContext.Update(hub);
        await dbContext.SaveChangesAsync();
        return hub;
    }

    private async Task<Track> SeedTrack(string title = "Test Song", string artist = "Test Artist")
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PlaybackDbContext>();

        var track = await TestDataSeeder.SeedTrack(dbContext,
            b => b.WithTitle(title).WithArtist(artist));
        return track;
    }

    private async Task<QueueEntry> SeedPlayingQueueEntry(
        Musicratic.Hub.Domain.Entities.Hub hub,
        Track track)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<PlaybackDbContext>();

        var entry = QueueEntry.Create(
            tenantId: hub.TenantId,
            trackId: track.Id,
            hubId: hub.Id,
            position: 0,
            source: QueueEntrySource.CoinProposal,
            proposerId: Guid.NewGuid(),
            costPaid: 10);
        entry.Play();

        dbContext.Add(entry);
        await dbContext.SaveChangesAsync();
        return entry;
    }

    private async Task SeedWalletWithBalance(Guid userId, Guid tenantId, decimal balance)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EconomyDbContext>();

        var wallet = Wallet.Create(userId, tenantId);
        wallet.Credit(balance, "Test funding");

        dbContext.Add(wallet);
        await dbContext.SaveChangesAsync();
    }

    private async Task<decimal> GetWalletBalance(Guid userId, Guid tenantId)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<EconomyDbContext>();

        var wallet = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstOrDefaultAsync(dbContext.Wallets,
                w => w.UserId == userId && w.TenantId == tenantId);

        return wallet?.Balance ?? throw new InvalidOperationException(
            $"Wallet not found for user {userId} in tenant {tenantId}");
    }

    private void OpenVotingWindow(Guid hubId, Guid entryId)
    {
        var votingWindowService = _factory.Services.GetRequiredService<IVotingWindowService>();
        votingWindowService.OpenWindow(hubId, entryId, TimeSpan.FromMinutes(5));
    }

    private static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize response to {typeof(T).Name}. Content: {content}");
    }

    // --- Response DTOs ---

    private sealed record ProposalResponse(
        Guid QueueEntryId,
        Guid TrackId,
        string Title,
        string Artist,
        string Status);

    private sealed record VoteTallyResponse(
        Guid QueueEntryId,
        int Upvotes,
        int Downvotes,
        int Total,
        double UpvotePercentage,
        double DownvotePercentage);

    private sealed record NowPlayingResponse(
        Guid QueueEntryId,
        Guid TrackId,
        string Title,
        string Artist);
}
