using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Hub.Application.Commands.AddListTrack;
using Musicratic.Hub.Application.Commands.CreateList;
using Musicratic.Hub.Application.Commands.DeleteList;
using Musicratic.Hub.Application.Commands.UpdateList;
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

    public sealed record CreateListRequest(string Name, Guid OwnerId, PlayMode PlayMode);

    public sealed record AddListTrackRequest(Guid TrackId);

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

    public sealed record UpdateListRequest(string Name, PlayMode PlayMode);
}
