using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Musicratic.Auth.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/auth").WithTags("Auth");

        group.MapGet("/login", LoginEndpoint.Login).WithName("Login");
        group.MapGet("/callback", CallbackEndpoint.Callback).WithName("Callback");

        return group;
    }
}
