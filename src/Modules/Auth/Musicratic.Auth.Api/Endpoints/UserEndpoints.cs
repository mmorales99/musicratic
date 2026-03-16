using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Auth.Application.Commands.UpdateProfile;
using Musicratic.Auth.Application.Queries.GetUserById;
using Musicratic.Auth.Application.Queries.GetUserBySub;

namespace Musicratic.Auth.Api.Endpoints;

public static class UserEndpoints
{
    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/me", GetCurrentUser).WithName("GetCurrentUser");
        group.MapGet("/{id:guid}", GetUserById).WithName("GetUserById");
        group.MapPut("/me", UpdateCurrentUserProfile).WithName("UpdateProfile");

        return group;
    }

    private static async Task<IResult> GetCurrentUser(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var sub = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub))
            return Results.Unauthorized();

        var user = await sender.Send(new GetUserBySubQuery(sub), cancellationToken);

        return user is null ? Results.NotFound() : Results.Ok(user);
    }

    private static async Task<IResult> GetUserById(
        Guid id,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var user = await sender.Send(new GetUserByIdQuery(id), cancellationToken);

        return user is null ? Results.NotFound() : Results.Ok(user);
    }

    private static async Task<IResult> UpdateCurrentUserProfile(
        UpdateProfileRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var sub = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub))
            return Results.Unauthorized();

        var user = await sender.Send(new GetUserBySubQuery(sub), cancellationToken);
        if (user is null)
            return Results.NotFound();

        await sender.Send(
            new UpdateProfileCommand(user.Id, request.DisplayName, request.AvatarUrl),
            cancellationToken);

        return Results.NoContent();
    }

    public sealed record UpdateProfileRequest(string DisplayName, string? AvatarUrl);
}
