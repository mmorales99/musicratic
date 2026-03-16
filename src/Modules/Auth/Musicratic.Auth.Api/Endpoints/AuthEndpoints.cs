using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Auth.Application.Commands.Logout;
using Musicratic.Auth.Application.Commands.RefreshToken;
using Musicratic.Auth.Infrastructure.Services;

namespace Musicratic.Auth.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Auth");

        group.MapGet("/login", LoginEndpoint.Login).WithName("Login");
        group.MapGet("/callback", CallbackEndpoint.Callback).WithName("Callback");
        group.MapPost("/refresh", Refresh).WithName("RefreshToken");
        group.MapPost("/logout", Logout).WithName("Logout").RequireAuthorization();

        return group;
    }

    private static async Task<IResult> Refresh(
        RefreshTokenRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Results.Problem(
                title: "Invalid request",
                detail: "Refresh token is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var result = await sender.Send(
                new RefreshTokenCommand(request.RefreshToken), cancellationToken);

            return Results.Ok(new
            {
                access_token = result.AccessToken,
                refresh_token = result.RefreshToken,
                expires_in = result.ExpiresIn
            });
        }
        catch (TokenRefreshException ex) when (ex.StatusCode == 401)
        {
            return Results.Problem(
                title: "Expired refresh token",
                detail: "The refresh token has expired. Please log in again.",
                statusCode: StatusCodes.Status401Unauthorized);
        }
        catch (TokenRefreshException)
        {
            return Results.Problem(
                title: "Token refresh failed",
                detail: "Unable to refresh the token. Please try again.",
                statusCode: StatusCodes.Status400BadRequest);
        }
    }

    private static async Task<IResult> Logout(
        LogoutRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.RefreshToken))
        {
            return Results.Problem(
                title: "Invalid request",
                detail: "Refresh token is required.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        await sender.Send(new LogoutCommand(request.RefreshToken), cancellationToken);

        return Results.NoContent();
    }

    public sealed record RefreshTokenRequest(string RefreshToken);

    public sealed record LogoutRequest(string RefreshToken);
}
