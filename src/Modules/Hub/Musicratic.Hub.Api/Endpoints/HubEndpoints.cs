using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Hub.Application.Commands.ActivateHub;
using Musicratic.Hub.Application.Commands.CreateHub;
using Musicratic.Hub.Application.Commands.DeactivateHub;
using Musicratic.Hub.Application.DTOs;
using Musicratic.Hub.Application.Queries.GetActiveHubs;
using Musicratic.Hub.Application.Queries.GetHub;
using Musicratic.Hub.Application.Queries.GetHubMembers;
using Musicratic.Hub.Domain.Entities;
using Musicratic.Hub.Domain.Enums;

namespace Musicratic.Hub.Api.Endpoints;

public static class HubEndpoints
{
    public static RouteGroupBuilder MapHubEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/hubs").WithTags("Hubs");

        group.MapGet("/", GetActiveHubs).WithName("GetActiveHubs");
        group.MapGet("/{id:guid}", GetHubById).WithName("GetHubById");
        group.MapPost("/", CreateHub).WithName("CreateHub");
        group.MapPost("/{id:guid}/activate", ActivateHub).WithName("ActivateHub");
        group.MapPost("/{id:guid}/deactivate", DeactivateHub).WithName("DeactivateHub");
        group.MapGet("/{id:guid}/members", GetHubMembers).WithName("GetHubMembers");

        return group;
    }

    private static async Task<IResult> GetActiveHubs(
        ISender sender,
        CancellationToken cancellationToken)
    {
        var hubs = await sender.Send(new GetActiveHubsQuery(), cancellationToken);
        return Results.Ok(hubs);
    }

    private static async Task<IResult> GetHubById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var hub = await sender.Send(new GetHubQuery(id), cancellationToken);
        return hub is null ? Results.NotFound() : Results.Ok(hub);
    }

    private static async Task<IResult> CreateHub(
        CreateHubRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var settings = new HubSettings
        {
            AllowProposals = request.Settings.AllowProposals,
            AutoSkipThreshold = request.Settings.AutoSkipThreshold,
            VotingWindowSeconds = request.Settings.VotingWindowSeconds,
            MaxQueueSize = request.Settings.MaxQueueSize,
            AllowedProviders = request.Settings.AllowedProviders,
            EnableLocalStorage = request.Settings.EnableLocalStorage,
            AdsEnabled = request.Settings.AdsEnabled
        };

        var hubId = await sender.Send(
            new CreateHubCommand(request.Name, request.Type, request.OwnerId, settings),
            cancellationToken);

        return Results.Created($"/api/hubs/{hubId}", new { id = hubId });
    }

    private static async Task<IResult> ActivateHub(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ActivateHubCommand(id), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> DeactivateHub(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeactivateHubCommand(id), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetHubMembers(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var members = await sender.Send(new GetHubMembersQuery(id), cancellationToken);
        return Results.Ok(members);
    }

    public sealed record CreateHubRequest(
        string Name,
        HubType Type,
        Guid OwnerId,
        HubSettingsRequest Settings);

    public sealed record HubSettingsRequest(
        bool AllowProposals = true,
        double AutoSkipThreshold = 0.65,
        int VotingWindowSeconds = 60,
        int MaxQueueSize = 50,
        List<MusicProvider>? AllowedProviders = null,
        bool EnableLocalStorage = false,
        bool AdsEnabled = false)
    {
        public List<MusicProvider> AllowedProviders { get; init; } =
            AllowedProviders ?? [MusicProvider.Spotify];
    }
}
