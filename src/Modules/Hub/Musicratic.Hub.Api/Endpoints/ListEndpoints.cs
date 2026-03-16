using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Hub.Application.Commands.AddListTrack;
using Musicratic.Hub.Application.Commands.CreateList;
using Musicratic.Hub.Domain.Enums;

namespace Musicratic.Hub.Api.Endpoints;

public static class ListEndpoints
{
    public static RouteGroupBuilder MapListEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/hubs/{hubId:guid}/lists").WithTags("Lists");

        group.MapPost("/", CreateList).WithName("CreateList");
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
}
