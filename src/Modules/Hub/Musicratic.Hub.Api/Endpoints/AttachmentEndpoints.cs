using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Hub.Application.Commands.AttachUser;
using Musicratic.Hub.Application.Commands.DetachUser;

namespace Musicratic.Hub.Api.Endpoints;

public static class AttachmentEndpoints
{
    public static RouteGroupBuilder MapAttachmentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/hubs").WithTags("Attachments");

        group.MapPost("/attach", AttachUser).WithName("AttachUser");
        group.MapPost("/detach", DetachUser).WithName("DetachUser");

        return group;
    }

    private static async Task<IResult> AttachUser(
        AttachRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userIdClaim = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(userIdClaim))
            return Results.Unauthorized();

        // In production, UserId would be resolved from the JWT sub claim via Auth module
        var attachmentId = await sender.Send(
            new AttachUserCommand(request.Code, request.UserId),
            cancellationToken);

        return Results.Ok(new { attachmentId });
    }

    private static async Task<IResult> DetachUser(
        DetachRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DetachUserCommand(request.UserId), cancellationToken);
        return Results.NoContent();
    }

    public sealed record AttachRequest(string Code, Guid UserId);

    public sealed record DetachRequest(Guid UserId);
}
