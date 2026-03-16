using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Auth.Application.Commands.UpdateProfile;
using Musicratic.Auth.Application.Commands.UploadAvatar;
using Musicratic.Auth.Application.Queries.GetUserById;
using Musicratic.Auth.Application.Queries.GetUserBySub;

namespace Musicratic.Auth.Api.Endpoints;

public static class UserEndpoints
{
    private const long MaxAvatarSize = 2 * 1024 * 1024; // 2 MB

    public static RouteGroupBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/me", GetCurrentUser).WithName("GetCurrentUser");
        group.MapGet("/{id:guid}", GetUserById).WithName("GetUserById");
        group.MapPut("/me", UpdateCurrentUserProfile).WithName("UpdateProfile");
        group.MapPost("/me/avatar", UploadAvatar)
            .WithName("UploadAvatar")
            .RequireAuthorization()
            .DisableAntiforgery();

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

    private static async Task<IResult> UploadAvatar(
        IFormFile file,
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

        try
        {
            await using var stream = file.OpenReadStream();

            var avatarUrl = await sender.Send(
                new UploadAvatarCommand(
                    user.Id,
                    stream,
                    file.FileName,
                    file.ContentType,
                    file.Length),
                cancellationToken);

            return Results.Ok(new { avatar_url = avatarUrl });
        }
        catch (ArgumentException ex)
        {
            return Results.Problem(
                title: "Invalid file",
                detail: ex.Message,
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    public sealed record UpdateProfileRequest(string DisplayName, string? AvatarUrl);
}
