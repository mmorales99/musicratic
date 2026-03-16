using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Musicratic.BFF.Web.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapWebAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/web/auth").WithTags("Web Auth");

        group.MapGet("/me", GetCurrentUser).WithName("WebGetCurrentUser");
        group.MapPut("/profile", UpdateProfile).WithName("WebUpdateProfile");

        return group;
    }

    private static async Task<IResult> GetCurrentUser(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var sub = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub))
            return Results.Unauthorized();

        // Forward to Auth module via Dapr service invocation or internal HTTP
        // In production, this uses DaprClient.InvokeMethodAsync to call Auth module
        return Results.Ok(new { message = "Forward to Auth.GetUserBySub", sub });
    }

    private static async Task<IResult> UpdateProfile(
        UpdateProfileRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var sub = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub))
            return Results.Unauthorized();

        // Forward to Auth module via Dapr service invocation
        return Results.NoContent();
    }

    public sealed record UpdateProfileRequest(string DisplayName, string? AvatarUrl);
}
