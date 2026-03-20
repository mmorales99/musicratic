[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Musicratic.Core.Tests")]

namespace Musicratic.Core.Models;

// ── Enums ────────────────────────────────────────────────────────────

public enum PlaybackState { Stopped, Playing, FadingOut }

public enum VoteOutcome { Passed, Rejected, OwnerVeto }

/// <summary>Phase A = voting during current song's last 45 s. Phase B = first 15 s of the suggested song.</summary>
public enum VotingPhase { PreTransition, PostTransition }

// ── Config ───────────────────────────────────────────────────────────

public class HubConfig
{
    private TimeSpan _fadeOutDuration = TimeSpan.FromSeconds(10);

    /// <summary>Fade-out duration when a suggested song is rejected mid-play. Clamped to 5–15 s.</summary>
    public TimeSpan FadeOutDuration
    {
        get => _fadeOutDuration;
        set => _fadeOutDuration = value.TotalSeconds switch
        {
            < 5 => TimeSpan.FromSeconds(5),
            > 15 => TimeSpan.FromSeconds(15),
            _ => value,
        };
    }

    public static readonly TimeSpan VotingPreTransitionWindow = TimeSpan.FromSeconds(45);
    public static readonly TimeSpan VotingPostTransitionWindow = TimeSpan.FromSeconds(15);
    public TimeSpan InactivityTimeout { get; set; } = TimeSpan.FromSeconds(15);
    public static readonly double RejectionThreshold = 0.65;
}

// ── Domain models ────────────────────────────────────────────────────

public class User
{
    public string Id { get; set; } = string.Empty;
}

public class Owner : User { }

public class Track
{
    public string Id { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }

    public static readonly TimeSpan MaxDuration = TimeSpan.FromMinutes(6);
}

// ── Voting ───────────────────────────────────────────────────────────

public class Voting
{
    public Track Track { get; set; } = null!;
    public int UpVotes { get; set; }
    public int DownVotes { get; set; }
    public TimeSpan TimeLeft { get; set; } = VotingDuration;
    public VotingPhase Phase { get; set; } = VotingPhase.PreTransition;
    public DateTime LastVoteTime { get; private set; }

    public bool IsActive => TimeLeft > TimeSpan.Zero;
    private bool IsOwnerDownVote { get; set; }
    private Timer? VotingTimer { get; set; }
    private Action<Voting>? OnCompleted { get; set; }
    private bool _completed;

    public void StartVoting(Action<Voting> onCompleted, HubConfig? config = null)
    {
        OnCompleted = onCompleted;
        LastVoteTime = DateTime.UtcNow;
        var inactivityTimeout = config?.InactivityTimeout ?? TimeSpan.FromSeconds(15);

        VotingTimer = new Timer(_ =>
        {
            TimeLeft = TimeLeft.Subtract(TimeSpan.FromSeconds(1));

            // Early termination: ≥65 % downvotes sustained for inactivityTimeout with no new votes
            if (TotalVotes > 0
                && DownVoteRatio >= HubConfig.RejectionThreshold
                && (DateTime.UtcNow - LastVoteTime) >= inactivityTimeout)
            {
                Complete();
                return;
            }

            if (TimeLeft <= TimeSpan.Zero)
            {
                Complete();
            }
        }, null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    public void CastVote(bool isUpVote, User user)
    {
        if (!IsActive)
            throw new InvalidOperationException("Voting has ended.");

        if (isUpVote)
            UpVotes++;
        else
            DownVotes++;

        LastVoteTime = DateTime.UtcNow;

        // If Owner votes down, just Owner vote is taken into account and the voting ends immediately
        if (user is Owner && !isUpVote)
        {
            IsOwnerDownVote = true;
            Complete();
        }
    }

    public VoteOutcome GetVoteResult()
    {
        if (IsOwnerDownVote)
            return VoteOutcome.OwnerVeto;

        // 0 votes = treated as upvoted → passed
        if (TotalVotes == 0)
            return VoteOutcome.Passed;

        return DownVoteRatio >= HubConfig.RejectionThreshold
            ? VoteOutcome.Rejected
            : VoteOutcome.Passed;
    }

    public int TotalVotes => UpVotes + DownVotes;
    public double DownVoteRatio => TotalVotes == 0 ? 0 : DownVotes / (double)TotalVotes;

    public static readonly TimeSpan VotingDuration = TimeSpan.FromSeconds(60);

    /// <summary>Force-end voting from outside (e.g. when the suggested song's 15 s overlap is reached).</summary>
    public void ForceComplete() => Complete();

    private void Complete()
    {
        if (_completed) return;
        _completed = true;
        TimeLeft = TimeSpan.Zero;
        VotingTimer?.Dispose();
        VotingTimer = null;
        OnCompleted?.Invoke(this);
    }
}

// ── Hub (main aggregate) ─────────────────────────────────────────────

public class HubFactory(Musicratic.Core.Ports.IHubRepository hubs)
{
    public async Task<Hub> GetOrCreateAsync(string id, User owner, bool isPublic = false)
    {
        var existing = await hubs.GetByIdAsync(id);
        if (existing is not null) return existing;

        var hub = new Hub
        {
            Id = id,
            Owner = new Owner { Id = owner.Id },
            IsPublic = isPublic,
            Config = new HubConfig(),
            Tracks = [],
            Queue = [],
            BannedTracks = [],
            SuggestedTracks = [],
            AttachedUsers = []
        };
        await hubs.AddAsync(hub);
        return hub;
    }
}

public class Hub
{
    public string Id { get; set; } = string.Empty;
    public Owner Owner { get; set; } = null!;
    public bool IsPublic { get; set; }
    public HubConfig Config { get; set; } = new();

    // Track library & queue
    public List<Track> Tracks { get; set; } = [];
    public List<Track> Queue { get; set; } = [];
    public List<Track> BannedTracks { get; set; } = [];

    // Playback
    public PlaybackState State { get; internal set; } = PlaybackState.Stopped;
    public Track? PlayingTrack { get; internal set; }
    public TimeSpan PlayingTrackPosition { get; internal set; }
    private Timer? PlaybackTimer { get; set; }

    // Suggestions & voting
    public List<Track> SuggestedTracks { get; set; } = [];
    public Voting? CurrentVoting { get; private set; }
    public bool IsSuggestedTrackPlaying { get; private set; }
    private bool HasDeferredSuggestion { get; set; }
    private bool VotingTriggeredForCurrentSong { get; set; }

    // Users
    public List<User> AttachedUsers { get; set; } = [];

    /// <summary>Suggestions are only allowed when at least one queue track is ≥ 45 s.</summary>
    public bool CanSuggest =>
        Queue.Exists(t => t.Duration >= HubConfig.VotingPreTransitionWindow)
        || Tracks.Exists(t => t.Duration >= HubConfig.VotingPreTransitionWindow);

    // ── Track list ───────────────────────────────────────────────────

    public void SetTrackList(List<Track> tracks)
    {
        Tracks = tracks;
        Queue = [.. tracks];
    }

    public void BanTrack(Track track)
    {
        if (!BannedTracks.Contains(track))
            BannedTracks.Add(track);
    }

    public void UnbanTrack(Track track)
    {
        BannedTracks.Remove(track);
    }

    // ── Playback ─────────────────────────────────────────────────────

    public void Play()
    {
        var next = Queue.FirstOrDefault();
        if (next == null)
        {
            Stop();
            return;
        }

        StartPlaying(next);
    }

    public void Stop()
    {
        StopTimer();
        PlayingTrack = null;
        PlayingTrackPosition = TimeSpan.Zero;
        State = PlaybackState.Stopped;
        IsSuggestedTrackPlaying = false;
        VotingTriggeredForCurrentSong = false;
    }

    public void Next()
    {
        StopTimer();
        if (PlayingTrack != null && !IsSuggestedTrackPlaying)
            Queue.Remove(PlayingTrack);

        IsSuggestedTrackPlaying = false;
        VotingTriggeredForCurrentSong = false;
        Play();
    }

    public void Shuffle()
    {
        Queue = [.. Queue.OrderBy(_ => Random.Shared.Next())];
    }

    public void ClearQueue()
    {
        Queue.Clear();
    }

    // ── Users ────────────────────────────────────────────────────────

    public void AttachUser(User user)
    {
        if (!AttachedUsers.Contains(user))
            AttachedUsers.Add(user);
    }

    public void DetachUser(User user)
    {
        AttachedUsers.Remove(user);
    }

    // ── Suggestions & voting ─────────────────────────────────────────

    public void SuggestTrack(Track track)
    {
        if (track.Duration > Track.MaxDuration)
            throw new InvalidOperationException($"Track duration exceeds the maximum allowed duration of {Track.MaxDuration.TotalMinutes} minutes.");

        if (BannedTracks.Contains(track))
            throw new InvalidOperationException("This track is banned and cannot be suggested.");

        if (!CanSuggest)
            throw new InvalidOperationException("No eligible song in queue is long enough to allow suggestions.");

        if (SuggestedTracks.Contains(track))
            return;

        SuggestedTracks.Add(track);

        // Decide if voting should start now or be deferred
        if (PlayingTrack == null || State == PlaybackState.Stopped)
        {
            TriggerVoting();
            return;
        }

        if (CurrentVoting != null)
            return; // A vote is already in progress; this suggestion waits in the FIFO queue

        var remaining = PlayingTrack.Duration - PlayingTrackPosition;
        var songDuration = PlayingTrack.Duration;

        if (songDuration < HubConfig.VotingPreTransitionWindow)
        {
            // Current song is too short — defer to a future eligible song
            HasDeferredSuggestion = true;
            return;
        }

        if (remaining <= HubConfig.VotingPreTransitionWindow
            && remaining >= TimeSpan.FromSeconds(30))
        {
            // 30 s – 45 s remaining → start voting immediately
            TriggerVoting();
        }
        else if (remaining < TimeSpan.FromSeconds(30))
        {
            // < 30 s remaining → defer to next eligible song
            HasDeferredSuggestion = true;
        }
        // > 45 s remaining → playback timer will trigger at the 45 s mark
    }

    public void CastVote(bool isUpVote, User user)
    {
        if (CurrentVoting == null)
            throw new InvalidOperationException("No active voting to cast a vote on.");

        // Resolve to Owner instance so the veto logic triggers correctly
        var voter = user.Id == Owner.Id ? Owner : user;
        CurrentVoting.CastVote(isUpVote, voter);
    }

    // ── Internal: playback tick ──────────────────────────────────────

    private void OnPlaybackTick()
    {
        if (PlayingTrack == null || State != PlaybackState.Playing)
            return;

        PlayingTrackPosition = PlayingTrackPosition.Add(TimeSpan.FromSeconds(1));
        var remaining = PlayingTrack.Duration - PlayingTrackPosition;

        // ─ Suggested song: check if 15 s overlap is reached ─
        if (IsSuggestedTrackPlaying && CurrentVoting is { IsActive: true })
        {
            if (PlayingTrackPosition >= HubConfig.VotingPostTransitionWindow)
                FinalizeVoting();
        }

        // ─ Suggested song ends (shorter than 15 s edge case, or normal end) ─
        if (IsSuggestedTrackPlaying && remaining <= TimeSpan.Zero)
        {
            OnSuggestedSongEnd();
            return;
        }

        // ─ Normal song end ─
        if (!IsSuggestedTrackPlaying && remaining <= TimeSpan.Zero)
        {
            OnNormalSongEnd();
            return;
        }

        // ─ 45 s mark: trigger voting for pending suggestion ─
        if (!IsSuggestedTrackPlaying
            && !VotingTriggeredForCurrentSong
            && CurrentVoting == null
            && SuggestedTracks.Count > 0
            && PlayingTrack.Duration >= HubConfig.VotingPreTransitionWindow
            && remaining <= HubConfig.VotingPreTransitionWindow)
        {
            VotingTriggeredForCurrentSong = true;
            TriggerVoting();
        }
    }

    // ── Internal: song transitions ───────────────────────────────────

    private void OnNormalSongEnd()
    {
        StopTimer();

        // Voting is active in Phase A → transition to Phase B: play the suggested song
        if (CurrentVoting is { IsActive: true, Phase: VotingPhase.PreTransition })
        {
            CurrentVoting.Phase = VotingPhase.PostTransition;
            Queue.Remove(PlayingTrack!);
            StartPlaying(CurrentVoting.Track, isSuggestion: true);
            return;
        }

        // No active voting — advance queue normally
        Queue.Remove(PlayingTrack!);
        VotingTriggeredForCurrentSong = false;

        // Check deferred suggestions against the new head track
        if (HasDeferredSuggestion && SuggestedTracks.Count > 0)
        {
            var next = Queue.FirstOrDefault();
            if (next != null && next.Duration >= HubConfig.VotingPreTransitionWindow)
            {
                HasDeferredSuggestion = false;
                // Timer will trigger voting at the 45 s mark of this new song
            }
        }

        Play();
    }

    private void OnSuggestedSongEnd()
    {
        StopTimer();

        // If voting is still running (song was shorter than 15 s), finalize it now
        if (CurrentVoting is { IsActive: true })
            FinalizeVoting();

        IsSuggestedTrackPlaying = false;
        VotingTriggeredForCurrentSong = false;
        Play(); // Resume normal queue (queue was never modified for the suggestion)
    }

    // ── Internal: voting lifecycle ───────────────────────────────────

    private void TriggerVoting()
    {
        if (SuggestedTracks.Count == 0 || CurrentVoting != null)
            return;

        var voting = new Voting { Track = SuggestedTracks[0] };
        CurrentVoting = voting;
        voting.StartVoting(OnVotingEnd, Config);
    }

    private void FinalizeVoting()
    {
        if (CurrentVoting == null)
            return;

        // Stop the voting timer and evaluate the final tally
        CurrentVoting.ForceComplete();
    }

    private void OnVotingEnd(Voting voting)
    {
        if (CurrentVoting != voting)
            return; // Stale callback — already handled

        var outcome = voting.GetVoteResult();
        var phase = voting.Phase;

        SuggestedTracks.Remove(voting.Track);
        CurrentVoting = null;

        switch (phase)
        {
            case VotingPhase.PreTransition:
                // Song hasn't started playing yet
                if (outcome is VoteOutcome.Rejected or VoteOutcome.OwnerVeto)
                {
                    // Suggestion discarded — queue continues as-is
                }
                else
                {
                    // Passed: the suggested song will start playing when the current song ends.
                    // Re-insert into SuggestedTracks head so OnNormalSongEnd picks it up.
                    SuggestedTracks.Insert(0, voting.Track);
                    // Re-create a completed voting so OnNormalSongEnd can find the track
                    CurrentVoting = voting;
                }
                break;

            case VotingPhase.PostTransition:
                // Song is already playing
                if (outcome is VoteOutcome.Rejected or VoteOutcome.OwnerVeto)
                    FadeOutAndResumeQueue();
                // Passed: let the song finish naturally (OnSuggestedSongEnd will resume queue)
                break;
        }
    }

    // ── Internal: fade-out ───────────────────────────────────────────

    private void FadeOutAndResumeQueue()
    {
        StopTimer();
        State = PlaybackState.FadingOut;

        // Fade-out timer: after the configured duration, resume the normal queue
        var fadeTimer = new Timer(_ =>
        {
            IsSuggestedTrackPlaying = false;
            VotingTriggeredForCurrentSong = false;
            Play();
        }, null, Config.FadeOutDuration, Timeout.InfiniteTimeSpan);
    }

    // ── Internal: timer helpers ──────────────────────────────────────

    private void StartPlaying(Track track, bool isSuggestion = false)
    {
        StopTimer();
        PlayingTrack = track;
        PlayingTrackPosition = TimeSpan.Zero;
        IsSuggestedTrackPlaying = isSuggestion;
        VotingTriggeredForCurrentSong = false;
        State = PlaybackState.Playing;

        PlaybackTimer = new Timer(_ => SimulateTick(),
            null, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
    }

    /// <summary>Advance playback by one second. Internal so tests can drive it synchronously.</summary>
    internal void SimulateTick() => OnPlaybackTick();

    private void StopTimer()
    {
        PlaybackTimer?.Dispose();
        PlaybackTimer = null;
    }
}