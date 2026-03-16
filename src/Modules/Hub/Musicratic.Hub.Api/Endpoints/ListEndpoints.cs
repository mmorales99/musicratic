using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Hub.Application.Commands.AddListTrack;
using Musicratic.Hub.Application.Commands.BulkAddListTracks;
using Musicratic.Hub.Application.Commands.CreateList;
using Musicratic.Hub.Application.Commands.DeleteList;
using Musicratic.Hub.Application.Commands.RemoveListTrack;
using Musicratic.Hub.Application.Commands.ReorderListTrack;
using Musicratic.Hub.Application.Commands.SetPlayMode;
using Musicratic.Hub.Application.Commands.UpdateList;
using Musicratic.Hub.Application.Queries.GetNextTrack;
using Musicratic.Hub.Domain.Enums;

namespace Musicratic.Hub.Api.Endpoints;

public static class ListEndpoints
{
    public static RouteGroupBuilder MapListEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/hubs/{hubId:guid}/lists").WithTags("Lists");

        group.MapPost("/", CreateList).WithName("CreateList");
        group.MapPut("/{listId:guid}", UpdateList).WithName("UpdateList");
        group.MapDelete("/{listId:guid}", DeleteList).WithName("DeleteList");
        group.MapPost("/{listId:guid}/tracks", AddListTrack).WithName("AddListTrack");
        group.MapDelete("/{listId:guid}/tracks/{trackId:guid}", RemoveListTrack).WithName("RemoveListTrack");
        group.MapPut("/{listId:guid}/tracks/{trackId:guid}/position", ReorderListTrack).WithName("ReorderListTrack");
        group.MapPost("/{listId:guid}/tracks/bulk", BulkAddListTracks).WithName("BulkAddListTracks");
        group.MapPut("/{listId:guid}/play-mode", SetPlayMode).WithName("SetPlayMode");
        group.MapGet("/{listId:guid}/next-track", GetNextTrack).WithName("GetNextTrack");

        return group;
    }

    private static async Task<IResult> CreateList(
        Guid hubId,
        CreateListRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var listId = await sender.Send(
            new CreateListCommand(hubId, request.Name, request.OwnerId, request.PlayMode),
            cancellationToken);

        return Results.Created($"/api/hubs/{hubId}/lists/{listId}", new { id = listId });
    }

    private static async Task<IResult> AddListTrack(
        Guid hubId,
        Guid listId,
        AddListTrackRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new AddListTrackCommand(listId, request.TrackId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> RemoveListTrack(
        Guid hubId,
        Guid listId,
        Guid trackId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new RemoveListTrackCommand(listId, trackId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> ReorderListTrack(
        Guid hubId,
        Guid listId,
        Guid trackId,
        ReorderListTrackRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new ReorderListTrackCommand(listId, trackId, request.Position), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> BulkAddListTracks(
        Guid hubId,
        Guid listId,
        BulkAddListTracksRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new BulkAddListTracksCommand(listId, request.TrackIds), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> SetPlayMode(
        Guid hubId,
        Guid listId,
        SetPlayModeRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new SetPlayModeCommand(listId, request.PlayMode), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> GetNextTrack(
        Guid hubId,
        Guid listId,
        ISender sender,
        CancellationToken cancellationToken,
        Guid? currentTrackId = null)
    {
        var track = await sender.Send(new GetNextTrackQuery(listId, currentTrackId), cancellationToken);
        return track is null ? Results.NotFound() : Results.Ok(track);
    }

    private static async Task<IResult> UpdateList(
        Guid hubId,
        Guid listId,
        UpdateListRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new UpdateListCommand(listId, request.Name, request.PlayMode),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> DeleteList(
        Guid hubId,
        Guid listId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteListCommand(listId), cancellationToken);
        return Results.NoContent();
    }

    public sealed record CreateListRequest(string Name, Guid OwnerId, PlayMode PlayMode);

    public sealed record AddListTrackRequest(Guid TrackId);

    public sealed record ReorderListTrackRequest(int Position);

    public sealed record BulkAddListTracksRequest(IReadOnlyList<Guid> TrackIds);

    public sealed record SetPlayModeRequest(PlayMode PlayMode);

    public sealed record UpdateListRequest(string Name, PlayMode PlayMode);
}
