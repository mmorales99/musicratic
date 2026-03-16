namespace Musicratic.Voting.Application.Services;

/// <summary>
/// VOTE-006: Tracks voting windows per hub/queue entry.
/// Voting closes when: track ends, skip triggered, or window expires.
/// </summary>
public interface IVotingWindowService
{
    bool IsVotingOpen(Guid hubId, Guid queueEntryId);

    void OpenWindow(Guid hubId, Guid queueEntryId, TimeSpan? duration = null);

    void CloseWindow(Guid hubId, Guid queueEntryId);
}
