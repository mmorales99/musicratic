using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Musicratic.BFF.Mobile.Endpoints;

public static class HubEndpoints
{
    public static RouteGroupBuilder MapMobileHubEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/mobile/hubs").WithTags("Mobile Hubs");

        group.MapGet("/", GetActiveHubs).WithName("MobileGetActiveHubs");
        group.MapGet("/{id:guid}", GetHub).WithName("MobileGetHub");
        group.MapPost("/", CreateHub).WithName("MobileCreateHub");
        group.MapPost("/attach", AttachUser).WithName("MobileAttachUser");
        group.MapPost("/detach", DetachUser).WithName("MobileDetachUser");

        return group;
    }

    private static Task<IResult> GetActiveHubs(CancellationToken cancellationToken)
    {
        // Forward to Hub module via Dapr service invocation
        return Task.FromResult(Results.Ok(new { message = "Forward to Hub.GetActiveHubs" }));
    }

    private static Task<IResult> GetHub(Guid id, CancellationToken cancellationToken)
    {
        // Forward to Hub module via Dapr service invocation
        return Task.FromResult(Results.Ok(new { message = "Forward to Hub.GetHub", hubId = id }));
    }

    private static Task<IResult> CreateHub(
        CreateHubRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var sub = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub))
            return Task.FromResult(Results.Unauthorized());

        // Forward to Hub module via Dapr service invocation
        return Task.FromResult(Results.Created("/api/mobile/hubs/placeholder", new { message = "Forward to Hub.CreateHub" }));
    }

    private static Task<IResult> AttachUser(
        AttachRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var sub = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub))
            return Task.FromResult(Results.Unauthorized());

        // Forward to Hub module via Dapr service invocation
        return Task.FromResult(Results.Ok(new { message = "Forward to Hub.AttachUser" }));
    }

    private static Task<IResult> DetachUser(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var sub = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub))
            return Task.FromResult(Results.Unauthorized());

        // Forward to Hub module via Dapr service invocation
        return Task.FromResult(Results.NoContent());
    }

    public sealed record CreateHubRequest(string Name, string Type, object Settings);

    public sealed record AttachRequest(string Code);
}
