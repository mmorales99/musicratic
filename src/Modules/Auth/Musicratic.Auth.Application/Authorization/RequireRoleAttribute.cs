using Musicratic.Auth.Domain.Enums;

namespace Musicratic.Auth.Application.Authorization;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public sealed class RequireRoleAttribute(UserRole minimumRole) : Attribute
{
    public UserRole MinimumRole { get; } = minimumRole;
}
