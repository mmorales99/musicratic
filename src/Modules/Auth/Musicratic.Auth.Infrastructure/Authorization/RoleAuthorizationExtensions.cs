using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Musicratic.Auth.Domain.Enums;

namespace Musicratic.Auth.Infrastructure.Authorization;

public static class RoleAuthorizationExtensions
{
    /// <summary>
    /// Registers the RoleAuthorizationMiddleware in the pipeline.
    /// Must be called after UseAuthentication() and UseAuthorization().
    /// </summary>
    public static IApplicationBuilder UseRoleAuthorization(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RoleAuthorizationMiddleware>();
    }

    /// <summary>
    /// Requires the caller to have at least the specified role.
    /// Usage: group.MapPost("/lists", CreateList).RequireRole(UserRole.ListOwner);
    /// </summary>
    public static RouteHandlerBuilder RequireRole(this RouteHandlerBuilder builder, UserRole minimumRole)
    {
        var filter = new RequireRoleEndpointFilter(minimumRole);
        return builder.AddEndpointFilter((context, next) => filter.InvokeAsync(context, next));
    }
}
