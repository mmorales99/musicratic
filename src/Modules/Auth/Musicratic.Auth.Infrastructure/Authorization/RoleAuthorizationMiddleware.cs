using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Musicratic.Auth.Application.Services;
using Musicratic.Auth.Domain.Enums;

namespace Musicratic.Auth.Infrastructure.Authorization;

/// <summary>
/// Middleware that resolves the current user's role per-request and stores it
/// in HttpContext.Items for downstream use by RequireRoleEndpointFilter.
/// See docs/07-user-roles.md for the 5-tier accumulative role system.
/// </summary>
public sealed class RoleAuthorizationMiddleware(
    RequestDelegate next,
    ILogger<RoleAuthorizationMiddleware> logger)
{
    private const string UserRoleKey = "UserRole";
    private const string MemberIdKey = "MemberId";

    public async Task InvokeAsync(HttpContext context)
    {
        var userId = GetUserId(context);
        var hubId = GetHubId(context);

        if (userId.HasValue && hubId.HasValue)
        {
            var roleService = context.RequestServices.GetRequiredService<IRoleResolutionService>();
            var resolved = await roleService.Resolve(userId.Value, hubId.Value, context.RequestAborted);

            context.Items[UserRoleKey] = resolved.Role;
            context.Items[MemberIdKey] = resolved.MemberId;

            logger.LogDebug(
                "Resolved role {Role} for user {UserId} in hub {HubId}",
                resolved.Role, userId.Value, hubId.Value);
        }
        else if (userId.HasValue)
        {
            context.Items[UserRoleKey] = UserRole.User;

            logger.LogDebug(
                "Authenticated user {UserId} without hub context, defaulting to User role",
                userId.Value);
        }
        else
        {
            context.Items[UserRoleKey] = UserRole.Anonymous;
        }

        await next(context);
    }

    private static Guid? GetUserId(HttpContext context)
    {
        var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? context.User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var userId) ? userId : null;
    }

    private static Guid? GetHubId(HttpContext context)
    {
        if (context.Request.RouteValues.TryGetValue("hubId", out var value)
            && value is not null
            && Guid.TryParse(value.ToString(), out var hubId))
        {
            return hubId;
        }

        return null;
    }
}
