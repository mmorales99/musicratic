using Microsoft.Extensions.Logging;
using Musicratic.Playback.Application.DTOs;
using Musicratic.Playback.Application.Services;

namespace Musicratic.Playback.Infrastructure.Services;

/// <summary>
/// Stub implementation of ICollectiveVoteService. Will be replaced
/// with Dapr service invocation to the Voting module.
/// </summary>
public sealed class CollectiveVoteService(
    ILogger<CollectiveVoteService> logger) : ICollectiveVoteService
{
    private const int DefaultVoteDurationMinutes = 2;
    private const double DefaultApprovalPercentage = 50.0;

    public Task<CooldownCheckDto> CheckCooldown(
        Guid tenantId,
        Guid proposerId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Wire to Voting module via Dapr service invocation
        // For now, always allow proposals
        logger.LogInformation(
            "Cooldown check stub: allowing proposal for user {ProposerId} in tenant {TenantId}",
            proposerId, tenantId);

        return Task.FromResult(new CooldownCheckDto(CanPropose: true));
    }

    public Task<CollectiveVoteResultDto> OpenVoteSession(
        Guid tenantId,
        Guid queueEntryId,
        Guid proposerId,
        CancellationToken cancellationToken = default)
    {
        // TODO: Wire to Voting module via Dapr service invocation
        // For now, return a mock vote session
        logger.LogInformation(
            "Opening stub vote session for entry {QueueEntryId} by {ProposerId}",
            queueEntryId, proposerId);

        var result = new CollectiveVoteResultDto(
            VoteSessionId: Guid.NewGuid(),
            ExpiresAt: DateTime.UtcNow.AddMinutes(DefaultVoteDurationMinutes),
            RequiredApprovalPercentage: DefaultApprovalPercentage);

        return Task.FromResult(result);
    }
}
