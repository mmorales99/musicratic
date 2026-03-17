using Musicratic.Auth.Domain.Enums;

namespace Musicratic.Auth.Application.Services;

public interface IRoleResolutionService
{
    Task<ResolvedRole> Resolve(Guid userId, Guid hubId, CancellationToken ct);
}

public sealed record ResolvedRole(UserRole Role, Guid? MemberId);
