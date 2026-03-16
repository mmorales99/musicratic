using System.Collections.Concurrent;

namespace Musicratic.Voting.Application.Services;

/// <summary>
/// VOTE-006: Enforces configurable voting window per hub.
/// Default window: 60 seconds (track duration for proposed tracks).
/// Voting closes when: track ends, skip triggered, or window expires.
/// </summary>
public sealed class VotingWindowService : IVotingWindowService
{
    private const int DefaultWindowSeconds = 60;

    private readonly ConcurrentDictionary<(Guid HubId, Guid QueueEntryId), VotingWindow> _windows = new();

    public bool IsVotingOpen(Guid hubId, Guid queueEntryId)
    {
        if (!_windows.TryGetValue((hubId, queueEntryId), out var window))
            return false;

        if (window.IsClosed)
            return false;

        return DateTime.UtcNow < window.ExpiresAt;
    }

    public void OpenWindow(Guid hubId, Guid queueEntryId, TimeSpan? duration = null)
    {
        var effectiveDuration = duration ?? TimeSpan.FromSeconds(DefaultWindowSeconds);
        var now = DateTime.UtcNow;

        _windows[(hubId, queueEntryId)] = new VotingWindow(now, now.Add(effectiveDuration));
    }

    public void CloseWindow(Guid hubId, Guid queueEntryId)
    {
        if (_windows.TryGetValue((hubId, queueEntryId), out var window))
        {
            _windows[(hubId, queueEntryId)] = window with { IsClosed = true };
        }
    }

    private sealed record VotingWindow(
        DateTime OpenedAt,
        DateTime ExpiresAt,
        bool IsClosed = false);
}
