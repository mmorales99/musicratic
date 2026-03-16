using Musicratic.Playback.Application.DTOs;

namespace Musicratic.Playback.Application.Services;

public interface ICollectiveVoteService
{
    Task<CooldownCheckDto> CheckCooldown(
        Guid tenantId,
        Guid proposerId,
        CancellationToken cancellationToken = default);

    Task<CollectiveVoteResultDto> OpenVoteSession(
        Guid tenantId,
        Guid queueEntryId,
        Guid proposerId,
        CancellationToken cancellationToken = default);
}
