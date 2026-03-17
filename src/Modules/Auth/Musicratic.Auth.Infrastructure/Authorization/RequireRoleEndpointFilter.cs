using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Musicratic.Auth.Domain.Enums;

namespace Musicratic.Auth.Infrastructure.Authorization;

/// <summary>
/// Endpoint filter that enforces a minimum role level on minimal API endpoints.
/// Reads the resolved role from HttpContext.Items (set by RoleAuthorizationMiddleware).
/// Returns 403 Problem Details (RFC 9457) if the current role is insufficient.
/// </summary>
public sealed class RequireRoleEndpointFilter(UserRole minimumRole) : IEndpointFilter
{
    public ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    {
        var currentRole = context.HttpContext.GetResolvedRole();

        if (currentRole >= minimumRole)
            return next(context);

        var logger = context.HttpContext.RequestServices
            .GetRequiredService<ILoggerFactory>()
            .CreateLogger<RequireRoleEndpointFilter>();

        logger.LogWarning(
            "Access denied: requires {RequiredRole}, current role is {CurrentRole}",
            minimumRole, currentRole);

        return new ValueTask<object?>(Results.Problem(
            type: "https://tools.ietf.org/html/rfc9457",
            title: "Forbidden",
            statusCode: StatusCodes.Status403Forbidden,
            detail: $"Requires minimum role: {minimumRole}. Current role: {currentRole}."));
    }
}
