using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Musicratic.Auth.Domain.Enums;

namespace Musicratic.Auth.Infrastructure.Authorization;

/// <summary>
/// Extension methods for reading resolved role data from HttpContext.Items.
/// Placed in Infrastructure (not Application) because it depends on ASP.NET Core HttpContext.
/// </summary>
public static class HttpContextRoleExtensions
{
    private const string UserRoleKey = "UserRole";
    private const string MemberIdKey = "MemberId";

    public static UserRole GetResolvedRole(this HttpContext context)
    {
        return context.Items.TryGetValue(UserRoleKey, out var role) && role is UserRole userRole
            ? userRole
            : UserRole.Anonymous;
    }

    public static Guid? GetMemberId(this HttpContext context)
    {
        return context.Items.TryGetValue(MemberIdKey, out var id) && id is Guid memberId
            ? memberId
            : null;
    }

    public static Guid? GetUserId(this HttpContext context)
    {
        var sub = context.User.FindFirstValue(ClaimTypes.NameIdentifier)
                  ?? context.User.FindFirstValue("sub");

        return Guid.TryParse(sub, out var userId) ? userId : null;
    }
}
