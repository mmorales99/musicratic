namespace Musicratic.Voting.Application.Services;

/// <summary>
/// VOTE-010: Anti-abuse rate limiting.
/// - Vote change cooldown: max 3 changes per user per track
/// - Rate limit: max 20 votes per user per minute across all tracks
/// </summary>
public interface IVoteRateLimiter
{
    bool CanVote(Guid userId, Guid queueEntryId, out string? reason);

    void RecordVote(Guid userId, Guid queueEntryId);
}
