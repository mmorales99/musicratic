using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Musicratic.BFF.Web.Endpoints;

public static class HubEndpoints
{
    public static RouteGroupBuilder MapWebHubEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/web/hubs").WithTags("Web Hubs");

        group.MapGet("/", GetActiveHubs).WithName("WebGetActiveHubs");
        group.MapGet("/{id:guid}", GetHub).WithName("WebGetHub");
        group.MapPost("/", CreateHub).WithName("WebCreateHub");
        group.MapPost("/attach", AttachUser).WithName("WebAttachUser");
        group.MapPost("/detach", DetachUser).WithName("WebDetachUser");

        return group;
    }

    private static async Task<IResult> GetActiveHubs(CancellationToken cancellationToken)
    {
        // Forward to Hub module via Dapr service invocation
        return Results.Ok(new { message = "Forward to Hub.GetActiveHubs" });
    }

    private static async Task<IResult> GetHub(Guid id, CancellationToken cancellationToken)
    {
        // Forward to Hub module via Dapr service invocation
        return Results.Ok(new { message = "Forward to Hub.GetHub", hubId = id });
    }

    private static async Task<IResult> CreateHub(
        CreateHubRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var sub = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub))
            return Results.Unauthorized();

        // Forward to Hub module via Dapr service invocation
        return Results.Created("/api/web/hubs/placeholder", new { message = "Forward to Hub.CreateHub" });
    }

    private static async Task<IResult> AttachUser(
        AttachRequest request,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var sub = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub))
            return Results.Unauthorized();

        // Forward to Hub module via Dapr service invocation
        return Results.Ok(new { message = "Forward to Hub.AttachUser" });
    }

    private static async Task<IResult> DetachUser(
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var sub = httpContext.User.FindFirst("sub")?.Value;
        if (string.IsNullOrEmpty(sub))
            return Results.Unauthorized();

        // Forward to Hub module via Dapr service invocation
        return Results.NoContent();
    }

    public sealed record CreateHubRequest(string Name, string Type, object Settings);

    public sealed record AttachRequest(string Code);
}
