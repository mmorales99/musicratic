using Musicratic.Voting.Application.DTOs;

namespace Musicratic.Voting.Application.Services;

/// <summary>
/// VOTE-009: Broadcasts vote tally updates via WebSocket.
/// Message types: VOTE_CAST, TALLY_UPDATED, SKIP_TRIGGERED.
/// </summary>
public interface IVoteTallyBroadcastService
{
    Task BroadcastVoteCast(
        Guid hubId, VoteTallyDto tally, CancellationToken cancellationToken = default);

    Task BroadcastTallyUpdated(
        Guid hubId, VoteTallyDto tally, CancellationToken cancellationToken = default);

    Task BroadcastSkipTriggered(
        Guid hubId, Guid queueEntryId, string reason, CancellationToken cancellationToken = default);
}
