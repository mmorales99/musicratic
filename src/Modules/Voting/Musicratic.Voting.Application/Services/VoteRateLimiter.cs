using System.Collections.Concurrent;

namespace Musicratic.Voting.Application.Services;

/// <summary>
/// VOTE-010: Anti-abuse rate limiting.
/// - Max 1 vote per user per queue entry (enforced by DB unique constraint)
/// - Vote change cooldown: max 3 changes per user per track (cast/remove cycles)
/// - Rate limit: max 20 votes per user per minute across all tracks
/// Uses in-memory ConcurrentDictionary (no external dependencies).
/// </summary>
public sealed class VoteRateLimiter : IVoteRateLimiter
{
    private const int MaxChangesPerTrack = 3;
    private const int MaxVotesPerMinute = 20;

    private readonly ConcurrentDictionary<(Guid UserId, Guid QueueEntryId), int> _changeCounters = new();
    private readonly ConcurrentDictionary<Guid, SlidingWindow> _minuteCounters = new();

    public bool CanVote(Guid userId, Guid queueEntryId, out string? reason)
    {
        var changeKey = (userId, queueEntryId);
        if (_changeCounters.TryGetValue(changeKey, out var changes) && changes >= MaxChangesPerTrack)
        {
            reason = $"Vote change limit exceeded. Maximum {MaxChangesPerTrack} changes per track.";
            return false;
        }

        var window = _minuteCounters.GetOrAdd(userId, _ => new SlidingWindow());
        window.PruneExpired();

        if (window.Count >= MaxVotesPerMinute)
        {
            reason = $"Rate limit exceeded. Maximum {MaxVotesPerMinute} votes per minute.";
            return false;
        }

        reason = null;
        return true;
    }

    public void RecordVote(Guid userId, Guid queueEntryId)
    {
        _changeCounters.AddOrUpdate(
            (userId, queueEntryId),
            1,
            (_, count) => count + 1);

        var window = _minuteCounters.GetOrAdd(userId, _ => new SlidingWindow());
        window.Record();
    }

    private sealed class SlidingWindow
    {
        private readonly ConcurrentQueue<DateTime> _timestamps = new();
        private static readonly TimeSpan WindowDuration = TimeSpan.FromMinutes(1);

        public int Count
        {
            get
            {
                PruneExpired();
                return _timestamps.Count;
            }
        }

        public void Record()
        {
            _timestamps.Enqueue(DateTime.UtcNow);
        }

        public void PruneExpired()
        {
            var cutoff = DateTime.UtcNow.Subtract(WindowDuration);
            while (_timestamps.TryPeek(out var oldest) && oldest < cutoff)
            {
                _timestamps.TryDequeue(out _);
            }
        }
    }
}
