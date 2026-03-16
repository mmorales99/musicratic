using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Notification.Application.Commands.RegisterDevice;
using Musicratic.Notification.Application.Commands.UnregisterDevice;
using Musicratic.Notification.Application.Queries.GetUserDevices;
using Musicratic.Notification.Domain.Enums;

namespace Musicratic.Notification.Api.Endpoints;

public static class DeviceEndpoints
{
    public static RouteGroupBuilder MapDeviceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/devices").WithTags("Devices");

        group.MapPost("/register", RegisterDevice).WithName("RegisterDevice");
        group.MapDelete("/{tokenId:guid}", UnregisterDevice).WithName("UnregisterDevice");
        group.MapGet("/", GetUserDevices).WithName("GetUserDevices");

        return group;
    }

    private static async Task<IResult> RegisterDevice(
        RegisterDeviceRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId(httpContext);
        if (userId is null)
            return Results.Unauthorized();

        if (string.IsNullOrWhiteSpace(request.Token))
            return Results.BadRequest("Device token is required.");

        var id = await sender.Send(
            new RegisterDeviceCommand(
                userId.Value,
                request.Token,
                request.Platform,
                request.DeviceName),
            cancellationToken);

        return Results.Ok(new { id });
    }

    private static async Task<IResult> UnregisterDevice(
        Guid tokenId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId(httpContext);
        if (userId is null)
            return Results.Unauthorized();

        await sender.Send(
            new UnregisterDeviceCommand(userId.Value, tokenId),
            cancellationToken);

        return Results.NoContent();
    }

    private static async Task<IResult> GetUserDevices(
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId(httpContext);
        if (userId is null)
            return Results.Unauthorized();

        var devices = await sender.Send(
            new GetUserDevicesQuery(userId.Value),
            cancellationToken);

        var response = devices.Select(d => new DeviceResponse(
            d.Id,
            d.Token,
            d.Platform.ToString(),
            d.DeviceName,
            d.IsActive,
            d.CreatedAt));

        return Results.Ok(new
        {
            success = true,
            total_items_in_response = devices.Count,
            has_more_items = false,
            items = response
        });
    }

    private static Guid? ExtractUserId(HttpContext context)
    {
        var sub = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                  ?? context.User.FindFirst("sub")?.Value;

        if (Guid.TryParse(sub, out var userId))
            return userId;

        return null;
    }
}

public sealed record RegisterDeviceRequest(
    string Token,
    DevicePlatform Platform,
    string? DeviceName = null);

public sealed record DeviceResponse(
    Guid Id,
    string Token,
    string Platform,
    string? DeviceName,
    bool IsActive,
    DateTime CreatedAt);
