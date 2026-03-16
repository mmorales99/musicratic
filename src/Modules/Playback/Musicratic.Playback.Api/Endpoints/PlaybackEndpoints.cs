using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Playback.Application.Commands.AddToQueue;
using Musicratic.Playback.Application.Commands.AdvanceTrack;
using Musicratic.Playback.Application.Commands.ApproveProposal;
using Musicratic.Playback.Application.Commands.ProposeTrackCollective;
using Musicratic.Playback.Application.Commands.RejectProposal;
using Musicratic.Playback.Application.Commands.SkipTrack;
using Musicratic.Playback.Application.Commands.StartPlayback;
using Musicratic.Playback.Application.Queries.GetNowPlaying;
using Musicratic.Playback.Application.Queries.GetQueue;
using Musicratic.Playback.Domain.Enums;

namespace Musicratic.Playback.Api.Endpoints;

public static class PlaybackEndpoints
{
    public static RouteGroupBuilder MapPlaybackEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/hubs/{hubId:guid}").WithTags("Playback");

        group.MapGet("/now-playing", GetNowPlaying).WithName("GetNowPlaying");
        group.MapGet("/queue", GetQueue).WithName("GetQueue");

        group.MapPost("/queue/propose", ProposeTrackCollective).WithName("ProposeTrackCollective");
        group.MapPost("/queue/propose-paid", ProposeTrackPaid).WithName("ProposeTrackPaid");
        group.MapPost("/queue/skip", SkipCurrentTrack).WithName("SkipCurrentTrack");

        group.MapPost("/queue/{entryId:guid}/approve", ApproveProposal).WithName("ApproveProposal");
        group.MapPost("/queue/{entryId:guid}/reject", RejectProposal).WithName("RejectProposal");

        group.MapPost("/playback/start", StartPlayback).WithName("StartPlayback");
        group.MapPost("/playback/advance", AdvanceTrack).WithName("AdvanceTrack");

        return group;
    }

    private static async Task<IResult> GetNowPlaying(
        Guid hubId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetNowPlayingQuery(hubId), cancellationToken);
        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static async Task<IResult> GetQueue(
        Guid hubId,
        int page,
        int pageSize,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var clampedPage = Math.Max(1, page);
        var clampedSize = Math.Clamp(pageSize, 1, 100);

        var result = await sender.Send(
            new GetQueueQuery(hubId, clampedPage, clampedSize), cancellationToken);

        return Results.Ok(new
        {
            Success = true,
            TotalItemsInResponse = result.Items.Count,
            HasMoreItems = result.HasMoreItems,
            Items = result.Items,
            TotalItems = result.TotalItems
        });
    }

    private static async Task<IResult> ProposeTrackCollective(
        Guid hubId,
        ProposeTrackCollectiveRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ProposeTrackCollectiveCommand(
            TenantId: hubId,
            HubId: hubId,
            TrackId: request.TrackId,
            ProposerId: request.ProposerId);

        var result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/hubs/{hubId}/queue/{result.QueueEntryId}", result);
    }

    private static Task<IResult> ProposeTrackPaid(
        Guid hubId,
        CancellationToken cancellationToken)
    {
        // PLAY-013 deferred — requires ECON-005
        return Task.FromResult(Results.StatusCode(StatusCodes.Status501NotImplemented));
    }

    private static async Task<IResult> SkipCurrentTrack(
        Guid hubId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new SkipTrackCommand(hubId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ApproveProposal(
        Guid hubId,
        Guid entryId,
        ApproveRejectRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new ApproveProposalCommand(hubId, entryId, request.UserId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> RejectProposal(
        Guid hubId,
        Guid entryId,
        ApproveRejectRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new RejectProposalCommand(hubId, entryId, request.UserId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> StartPlayback(
        Guid hubId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new StartPlaybackCommand(hubId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> AdvanceTrack(
        Guid hubId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new AdvanceTrackCommand(hubId), cancellationToken);
        return Results.NoContent();
    }
}

public sealed record ProposeTrackCollectiveRequest(Guid TrackId, Guid ProposerId);

public sealed record ApproveRejectRequest(Guid UserId);
