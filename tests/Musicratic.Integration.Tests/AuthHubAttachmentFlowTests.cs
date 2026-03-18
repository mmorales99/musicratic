using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Hub.Domain.Enums;
using Musicratic.Hub.Infrastructure.Persistence;
using Musicratic.TestUtilities.Auth;
using Musicratic.TestUtilities.Builders;
using Musicratic.TestUtilities.Seeders;

namespace Musicratic.Integration.Tests;

public class AuthHubAttachmentFlowTests : IClassFixture<CustomWebApplicationFactory>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly CustomWebApplicationFactory _factory;

    public AuthHubAttachmentFlowTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateHub_WithAuthenticatedUser_ReturnsCreatedHub()
    {
        // Arrange
        var client = CreateAuthenticatedClient(role: "hub_manager");
        var ownerId = TestAuthHandler.DefaultUserId;

        var request = new
        {
            Name = "Integration Test Hub",
            Type = (int)HubType.Venue,
            OwnerId = ownerId,
            Settings = new
            {
                AllowProposals = true,
                AutoSkipThreshold = 0.65,
                VotingWindowSeconds = 60,
                MaxQueueSize = 50,
                AllowedProviders = new[] { (int)MusicProvider.Spotify },
                EnableLocalStorage = false,
                AdsEnabled = false
            }
        };

        // Act
        var response = await client.PostAsJsonAsync("/api/hubs", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var body = await DeserializeResponse<HubCreatedResponse>(response);
        body.Id.Should().NotBeEmpty();
        response.Headers.Location?.ToString().Should().Contain($"/api/hubs/{body.Id}");
    }

    [Fact]
    public async Task AttachToHub_WithValidCode_AddsMemberToHub()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var hub = await SeedActiveHub();

        var client = CreateAuthenticatedClient(userId: userId.ToString());
        var attachRequest = new { Code = hub.Code, UserId = userId };

        // Act
        var response = await client.PostAsJsonAsync("/api/hubs/attach", attachRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await DeserializeResponse<AttachmentResponse>(response);
        body.AttachmentId.Should().NotBeEmpty();

        // Verify user appears in member list
        var membersResponse = await client.GetAsync($"/api/hubs/{hub.Id}/members");
        membersResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var members = await DeserializeResponse<PagedEnvelopeResponse<HubMemberResponse>>(membersResponse);
        members.Items.Should().Contain(m => m.UserId == userId);
    }

    [Fact]
    public async Task AttachToHub_AlreadyAttached_DetachesPreviousAndAttachesNew()
    {
        // Arrange — Attach user to first hub, then attach again to second hub
        var userId = Guid.NewGuid();
        var hub1 = await SeedActiveHub("Hub Alpha");
        var hub2 = await SeedActiveHub("Hub Beta");

        var client = CreateAuthenticatedClient(userId: userId.ToString());

        await client.PostAsJsonAsync("/api/hubs/attach", new { Code = hub1.Code, UserId = userId });

        // Act — attach same user to second hub
        var response = await client.PostAsJsonAsync("/api/hubs/attach", new { Code = hub2.Code, UserId = userId });

        // Assert — second attachment succeeds (handler detaches previous first)
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var body = await DeserializeResponse<AttachmentResponse>(response);
        body.AttachmentId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DetachFromHub_WhenMember_RemovesMembership()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var hub = await SeedActiveHub();

        var client = CreateAuthenticatedClient(userId: userId.ToString());
        await client.PostAsJsonAsync("/api/hubs/attach", new { Code = hub.Code, UserId = userId });

        // Act
        var detachResponse = await client.PostAsJsonAsync("/api/hubs/detach", new { UserId = userId });

        // Assert
        detachResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task DetachFromHub_WhenNotAttached_ReturnsNoContent()
    {
        // Arrange — user never attached
        var userId = Guid.NewGuid();
        var client = CreateAuthenticatedClient(userId: userId.ToString());

        // Act
        var response = await client.PostAsJsonAsync("/api/hubs/detach", new { UserId = userId });

        // Assert — handler is idempotent
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task GetHubMembers_AfterMultipleAttachments_ReturnsAllMembers()
    {
        // Arrange
        var hub = await SeedActiveHub();

        var user1Id = Guid.NewGuid();
        var user2Id = Guid.NewGuid();
        var user3Id = Guid.NewGuid();

        var client1 = CreateAuthenticatedClient(userId: user1Id.ToString());
        var client2 = CreateAuthenticatedClient(userId: user2Id.ToString());
        var client3 = CreateAuthenticatedClient(userId: user3Id.ToString());

        await client1.PostAsJsonAsync("/api/hubs/attach", new { Code = hub.Code, UserId = user1Id });
        await client2.PostAsJsonAsync("/api/hubs/attach", new { Code = hub.Code, UserId = user2Id });
        await client3.PostAsJsonAsync("/api/hubs/attach", new { Code = hub.Code, UserId = user3Id });

        // Act
        var response = await client1.GetAsync($"/api/hubs/{hub.Id}/members");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var members = await DeserializeResponse<PagedEnvelopeResponse<HubMemberResponse>>(response);
        members.Success.Should().BeTrue();
        members.Items.Should().Contain(m => m.UserId == user1Id);
        members.Items.Should().Contain(m => m.UserId == user2Id);
        members.Items.Should().Contain(m => m.UserId == user3Id);
        members.TotalItemsInResponse.Should().BeGreaterThanOrEqualTo(3);
    }

    [Fact]
    public async Task FullFlow_Register_CreateHub_Attach_VerifyMembership()
    {
        // Arrange — hub manager creates a hub
        var ownerId = Guid.NewGuid();
        var managerClient = CreateAuthenticatedClient(userId: ownerId.ToString(), role: "hub_manager");

        var createRequest = new
        {
            Name = "Full Flow Hub",
            Type = (int)HubType.Venue,
            OwnerId = ownerId,
            Settings = new
            {
                AllowProposals = true,
                AutoSkipThreshold = 0.65,
                VotingWindowSeconds = 60,
                MaxQueueSize = 50,
                AllowedProviders = new[] { (int)MusicProvider.Spotify },
                EnableLocalStorage = false,
                AdsEnabled = false
            }
        };

        var createResponse = await managerClient.PostAsJsonAsync("/api/hubs", createRequest);
        createResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var created = await DeserializeResponse<HubCreatedResponse>(createResponse);

        // Activate the hub before attaching
        var activateResponse = await managerClient.PostAsync($"/api/hubs/{created.Id}/activate", null);
        activateResponse.StatusCode.Should().Be(HttpStatusCode.NoContent);

        // Retrieve hub to get its code
        var hubResponse = await managerClient.GetAsync($"/api/hubs/{created.Id}");
        hubResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var hubData = await DeserializeResponse<HubDetailResponse>(hubResponse);

        // Act — visitor scans QR and attaches
        var visitorId = Guid.NewGuid();
        var visitorClient = CreateAuthenticatedClient(userId: visitorId.ToString(), role: "user");

        var attachResponse = await visitorClient.PostAsJsonAsync(
            "/api/hubs/attach",
            new { Code = hubData.Code, UserId = visitorId });

        attachResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert — verify visitor is now a member
        var membersResponse = await managerClient.GetAsync($"/api/hubs/{created.Id}/members");
        membersResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var members = await DeserializeResponse<PagedEnvelopeResponse<HubMemberResponse>>(membersResponse);
        members.Items.Should().Contain(m => m.UserId == visitorId);
    }

    // --- Helpers ---

    private HttpClient CreateAuthenticatedClient(
        string? userId = null,
        string role = "user")
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add(
            TestAuthHandler.UserIdHeader,
            userId ?? TestAuthHandler.DefaultUserId.ToString());
        client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeader, role);
        return client;
    }

    private async Task<Musicratic.Hub.Domain.Entities.Hub> SeedActiveHub(string name = "Seeded Hub")
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<HubDbContext>();

        var hub = await TestDataSeeder.SeedHub(dbContext, b => b.WithName(name));
        hub.Activate();
        dbContext.Update(hub);
        await dbContext.SaveChangesAsync();
        return hub;
    }

    private static async Task<T> DeserializeResponse<T>(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<T>(content, JsonOptions)
            ?? throw new InvalidOperationException(
                $"Failed to deserialize response to {typeof(T).Name}. Content: {content}");
    }

    // --- Response DTOs for deserialization ---

    private sealed record HubCreatedResponse(Guid Id);

    private sealed record AttachmentResponse(Guid AttachmentId);

    private sealed record HubDetailResponse(
        Guid Id,
        string Name,
        string Code,
        bool IsActive);

    private sealed record HubMemberResponse(
        Guid Id,
        Guid UserId,
        string? DisplayName,
        HubMemberRole Role,
        DateTime AssignedAt);

    private sealed record PagedEnvelopeResponse<T>(
        bool Success,
        int TotalItemsInResponse,
        bool HasMoreItems,
        List<T> Items);
}
