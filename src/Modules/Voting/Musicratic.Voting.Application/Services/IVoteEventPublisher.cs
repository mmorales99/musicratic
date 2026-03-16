using Musicratic.Voting.Domain.Enums;

namespace Musicratic.Voting.Application.Services;

/// <summary>
/// VOTE-013: Publishes voting integration events via Dapr pub/sub.
/// Topics: {env}_voting_vote-cast, {env}_voting_skip-triggered
/// </summary>
public interface IVoteEventPublisher
{
    Task PublishVoteCastAsync(
        Guid tenantId, Guid queueEntryId, Guid userId, VoteValue value,
        CancellationToken cancellationToken = default);

    Task PublishSkipTriggeredAsync(
        Guid tenantId, Guid queueEntryId, string reason, double downvotePercentage,
        CancellationToken cancellationToken = default);
}
