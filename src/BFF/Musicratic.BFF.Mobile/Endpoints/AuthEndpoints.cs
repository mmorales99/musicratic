using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Musicratic.BFF.Mobile.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapMobileAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/mobile/auth").WithTags("Mobile Auth");

        group.MapGet("/me", GetCurrentUser).WithName("MobileGetCurrentUser");
        group.MapPut("/profile", UpdateProfile).WithName("MobileUpdateProfile");

        return group;
    }

    private static Task<IResult> GetCurrentUser(HttpContext httpContext, CancellationToken cancellationToken)
    {
        var sub = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub))
            return Task.FromResult(Results.Unauthorized());

        // Forward to Auth module via Dapr service invocation
        return Task.FromResult(Results.Ok(new { message = "Forward to Auth.GetUserBySub", sub }));
    }

    private static Task<IResult> UpdateProfile(
        UpdateProfileRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var sub = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub))
            return Task.FromResult(Results.Unauthorized());

        // Forward to Auth module via Dapr service invocation
        return Task.FromResult(Results.NoContent());
    }

    public sealed record UpdateProfileRequest(string DisplayName, string? AvatarUrl);
}
