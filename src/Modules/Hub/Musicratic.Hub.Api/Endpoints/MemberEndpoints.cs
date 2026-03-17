using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Hub.Application.Commands.AssignListToMember;
using Musicratic.Hub.Application.Commands.DemoteMember;
using Musicratic.Hub.Application.Commands.PromoteMember;
using Musicratic.Hub.Application.Commands.UnassignListFromMember;
using Musicratic.Hub.Domain.Enums;

namespace Musicratic.Hub.Api.Endpoints;

public static class MemberEndpoints
{
    public static RouteGroupBuilder MapMemberEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/hubs/{hubId:guid}/members/{userId:guid}")
            .WithTags("Members");

        group.MapPut("/promote", PromoteMember).WithName("PromoteMember");
        group.MapPut("/demote", DemoteMember).WithName("DemoteMember");

        group.MapPost("/assigned-lists", AssignList).WithName("AssignListToMember");
        group.MapDelete("/assigned-lists/{listId:guid}", UnassignList).WithName("UnassignListFromMember");

        return group;
    }

    private static async Task<IResult> PromoteMember(
        Guid hubId,
        Guid userId,
        PromoteMemberRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        // TODO: resolve promotedBy from authenticated user context
        await sender.Send(
            new PromoteMemberCommand(hubId, userId, request.NewRole, request.PromotedBy),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> DemoteMember(
        Guid hubId,
        Guid userId,
        DemoteMemberRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        // TODO: resolve demotedBy from authenticated user context
        await sender.Send(
            new DemoteMemberCommand(hubId, userId, request.NewRole, request.DemotedBy),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> AssignList(
        Guid hubId,
        Guid userId,
        AssignListRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        // TODO: resolve assignedBy from authenticated user context
        await sender.Send(
            new AssignListToMemberCommand(hubId, userId, request.ListId, request.AssignedBy),
            cancellationToken);

        return Results.Created(
            $"/api/hubs/{hubId}/members/{userId}/assigned-lists/{request.ListId}",
            new { listId = request.ListId });
    }

    private static async Task<IResult> UnassignList(
        Guid hubId,
        Guid userId,
        Guid listId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(
            new UnassignListFromMemberCommand(hubId, userId, listId),
            cancellationToken);

        return Results.NoContent();
    }

    public sealed record PromoteMemberRequest(HubMemberRole NewRole, Guid PromotedBy);

    public sealed record DemoteMemberRequest(HubMemberRole NewRole, Guid DemotedBy);

    public sealed record AssignListRequest(Guid ListId, Guid AssignedBy);
}
