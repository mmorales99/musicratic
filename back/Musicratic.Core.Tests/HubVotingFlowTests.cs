using FluentAssertions;
using Musicratic.Core.Models;

namespace Musicratic.Core.Tests;

[Collection("MusicraticSuite")]
/// <summary>
/// Tests for the full suggestion + voting flow in Hub.
/// Uses SimulateTick() to drive playback synchronously without real timers.
/// Track t1 = 90 s, trigger window = 45 s, so voting fires at tick 45.
/// </summary>
public class HubVotingFlowTests
{
    // ── Helpers ───────────────────────────────────────────────────────

    private static Hub CreateHub(bool play = false)
    {
        var hub = new Hub
        {
            Id = "h1",
            Owner = new Owner { Id = "owner" },
            IsPublic = false,
            Config = new HubConfig(),
            Tracks = [],
            Queue = [],
            BannedTracks = [],
            SuggestedTracks = [],
            AttachedUsers = []
        };
        hub.SetTrackList([
            new Track { Id = "t1", Duration = TimeSpan.FromSeconds(90) },
            new Track { Id = "t2", Duration = TimeSpan.FromSeconds(90) },
        ]);
        if (play) hub.Play();
        return hub;
    }

    private static Track SuggestedTrack() =>
        new() { Id = "s1", Duration = TimeSpan.FromMinutes(2) };

    /// <summary>Drive hub forward by <paramref name="count"/> ticks synchronously.</summary>
    private static void Tick(Hub hub, int count = 1)
    {
        for (int i = 0; i < count; i++)
            hub.SimulateTick();
    }

    // ── Trigger window ────────────────────────────────────────────────

    [Fact]
    public void VotingTriggeredAt45sMarkOfCurrentSong()
    {
        var hub = CreateHub(play: true);
        hub.SuggestTrack(SuggestedTrack());

        // Before 45 s mark, no voting
        Tick(hub, 44);
        hub.CurrentVoting.Should().BeNull();

        // At tick 45, remaining = 45 s ≤ VotingPreTransitionWindow → trigger
        hub.SimulateTick();
        hub.CurrentVoting.Should().NotBeNull();
        hub.CurrentVoting!.Track.Id.Should().Be("s1");
        hub.CurrentVoting.Phase.Should().Be(VotingPhase.PreTransition);

        hub.CurrentVoting.ForceComplete();
    }

    [Fact]
    public void VotingNotTriggeredTwiceForSameSong()
    {
        var hub = CreateHub(play: true);
        hub.SuggestTrack(SuggestedTrack());

        Tick(hub, 45); // trigger
        var voting = hub.CurrentVoting;
        Tick(hub, 5);  // a few more ticks within the song

        // Still the same voting instance
        hub.CurrentVoting.Should().BeSameAs(voting);

        hub.CurrentVoting?.ForceComplete();
    }

    [Fact]
    public void SuggestInWindow_30To45sRemaining_TriggersImmediately()
    {
        var hub = CreateHub(play: true);

        // Advance to 50 s played (40 s remaining); remaining = 40 s → in [30 s, 45 s]
        Tick(hub, 50);
        hub.SuggestTrack(SuggestedTrack());

        hub.CurrentVoting.Should().NotBeNull();

        hub.CurrentVoting!.ForceComplete();
    }

    [Fact]
    public void SuggestWhenLessThan30sRemaining_DeferredToNextSong()
    {
        var hub = CreateHub(play: true);

        // Advance to 65 s played (25 s remaining)
        Tick(hub, 65);
        hub.SuggestTrack(SuggestedTrack());

        // No voting yet — deferred
        hub.CurrentVoting.Should().BeNull();
    }

    [Fact]
    public void SuggestWhileNotPlaying_TriggersVotingImmediately()
    {
        var hub = CreateHub(play: false); // hub is Stopped but has eligible queue
        hub.SuggestTrack(SuggestedTrack());

        hub.CurrentVoting.Should().NotBeNull();
        hub.CurrentVoting!.ForceComplete();
    }

    // ── Phase A outcomes ──────────────────────────────────────────────

    [Fact]
    public void PhaseA_Rejection_SuggestionDiscarded_QueueContinues()
    {
        var hub = CreateHub(play: true);
        hub.SuggestTrack(SuggestedTrack());

        Tick(hub, 45); // voting triggered
        // Cast 65 % downvotes (e.g. 0 up, 1 down = owner)
        hub.CastVote(isUpVote: false, user: new User { Id = "u1" });
        hub.CastVote(isUpVote: false, user: new User { Id = "u2" });

        // Force complete to evaluate outcome synchronously
        hub.CurrentVoting!.ForceComplete();

        hub.SuggestedTracks.Should().BeEmpty();
        hub.CurrentVoting.Should().BeNull();
        // Should still be playing t1
        hub.PlayingTrack!.Id.Should().Be("t1");
    }

    [Fact]
    public void PhaseA_OwnerVeto_SuggestionDiscardedImmediately()
    {
        var hub = CreateHub(play: true);
        hub.SuggestTrack(SuggestedTrack());

        Tick(hub, 45); // voting triggered
        hub.CastVote(isUpVote: false, user: new User { Id = hub.Owner.Id });

        // Veto → synchronous completion
        hub.SuggestedTracks.Should().BeEmpty();
        hub.CurrentVoting.Should().BeNull();
        hub.PlayingTrack!.Id.Should().Be("t1"); // still on t1
    }

    [Fact]
    public void PhaseA_Pass_SuggestedTrackStartsWhenCurrentSongEnds()
    {
        var hub = CreateHub(play: true);
        hub.SuggestTrack(SuggestedTrack());

        // Tick to 45 s mark → voting triggers
        Tick(hub, 45);
        hub.CurrentVoting.Should().NotBeNull();

        // Do NOT end the voting — let it remain Active while the song finishes
        // Tick the remaining 45 s to end the song (90 ticks total)
        Tick(hub, 45);

        // OnNormalSongEnd fires: voting still active (15 s left) → Phase B
        hub.IsSuggestedTrackPlaying.Should().BeTrue();
        hub.PlayingTrack!.Id.Should().Be("s1");
        hub.CurrentVoting!.Phase.Should().Be(VotingPhase.PostTransition);

        hub.CurrentVoting.ForceComplete();
    }

    // ── Phase B outcomes ──────────────────────────────────────────────

    [Fact]
    public void PhaseB_After15sTick_VotingFinalizedAutomatically()
    {
        var hub = CreateHub(play: true);
        hub.SuggestTrack(SuggestedTrack());

        Tick(hub, 90); // song ends → Phase B starts (voting still active)
        hub.IsSuggestedTrackPlaying.Should().BeTrue();

        // Tick 15 s into the suggested song → auto-finalize
        Tick(hub, 15);

        // Voting should be resolved
        hub.CurrentVoting.Should().BeNull();
        // Suggested song continues (passed) — still playing s1
        hub.IsSuggestedTrackPlaying.Should().BeTrue();
    }

    [Fact]
    public void PhaseB_Rejection_FadeOut()
    {
        var hub = CreateHub(play: true);
        hub.SuggestTrack(SuggestedTrack());

        Tick(hub, 90); // Phase B starts
        hub.IsSuggestedTrackPlaying.Should().BeTrue();

        // Cast rejection and force complete
        hub.CastVote(isUpVote: false, user: new User { Id = "u1" });
        hub.CastVote(isUpVote: false, user: new User { Id = "u2" });
        hub.CurrentVoting!.ForceComplete();

        hub.State.Should().Be(PlaybackState.FadingOut);
        hub.CurrentVoting.Should().BeNull();
    }

    [Fact]
    public void PhaseB_OwnerVeto_FadeOut()
    {
        var hub = CreateHub(play: true);
        hub.SuggestTrack(SuggestedTrack());

        Tick(hub, 90); // Phase B starts
        hub.CastVote(isUpVote: false, user: new User { Id = hub.Owner.Id });

        hub.State.Should().Be(PlaybackState.FadingOut);
    }

    [Fact]
    public void PhaseB_Pass_SuggestedSongContinues()
    {
        var hub = CreateHub(play: true);
        hub.SuggestTrack(SuggestedTrack());

        Tick(hub, 90); // Phase B starts
        hub.IsSuggestedTrackPlaying.Should().BeTrue();

        // All upvotes, then force complete
        hub.CastVote(isUpVote: true, user: new User { Id = "u1" });
        hub.CurrentVoting!.ForceComplete();

        // Song should still be playing
        hub.State.Should().Be(PlaybackState.Playing);
        hub.IsSuggestedTrackPlaying.Should().BeTrue();
        hub.PlayingTrack!.Id.Should().Be("s1");
    }

    // ── Suggested song shorter than 15 s window ───────────────────────

    [Fact]
    public void PhaseB_SongShorterThan15s_VotingFinalizedAtSongEnd()
    {
        var hub = new Hub
        {
            Id = "h1",
            Owner = new Owner { Id = "owner" },
            Config = new HubConfig(),
            Tracks = [], Queue = [], BannedTracks = [], SuggestedTracks = [], AttachedUsers = []
        };
        hub.SetTrackList([new Track { Id = "t1", Duration = TimeSpan.FromSeconds(90) }]);
        hub.Play();

        // Suggest a very short track (10 s)
        var shortSuggested = new Track { Id = "short", Duration = TimeSpan.FromSeconds(10) };
        hub.SuggestTrack(shortSuggested);

        Tick(hub, 90); // Phase B starts (shortSuggested begins playing)
        hub.IsSuggestedTrackPlaying.Should().BeTrue();

        // Tick 10 s → song ends before 15 s mark → OnSuggestedSongEnd finalizes voting
        Tick(hub, 10);

        hub.IsSuggestedTrackPlaying.Should().BeFalse();
        hub.CurrentVoting.Should().BeNull();
    }

    // ── FIFO ordering ─────────────────────────────────────────────────

    [Fact]
    public void MultipleSuggestions_ProcessedInFifoOrder()
    {
        var hub = CreateHub(play: false);
        var s1 = new Track { Id = "s1", Duration = TimeSpan.FromMinutes(2) };
        var s2 = new Track { Id = "s2", Duration = TimeSpan.FromMinutes(2) };

        hub.SuggestTrack(s1); // triggers voting immediately (stopped)
        hub.SuggestTrack(s2); // queued

        // First voting is for s1
        hub.CurrentVoting!.Track.Id.Should().Be("s1");
        hub.SuggestedTracks.Should().HaveCount(2);

        hub.CurrentVoting.ForceComplete();
    }

    // ── Queue preservation ────────────────────────────────────────────

    [Fact]
    public void SuggestionFlow_DoesNotModifyQueue_UntilPhaseB()
    {
        var hub = CreateHub(play: true);
        var originalQueue = hub.Queue.Select(t => t.Id).ToList();
        hub.SuggestTrack(SuggestedTrack());

        Tick(hub, 45); // voting triggered
        hub.Queue.Select(t => t.Id).Should().BeEquivalentTo(originalQueue);

        hub.CurrentVoting!.ForceComplete();
    }
}
