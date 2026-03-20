using FluentAssertions;
using Musicratic.Core.Models;

namespace Musicratic.Core.Tests;

[Collection("MusicraticSuite")]
public class HubDomainTests
{
    private readonly Hub _hub;

    public HubDomainTests()
    {
        _hub = new Hub
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
    }

    // ── Track list ────────────────────────────────────────────────────

    [Fact]
    public void SetTrackList_SetsTracksAndQueue()
    {
        var tracks = new List<Track>
        {
            new() { Id = "t1", Duration = TimeSpan.FromMinutes(3) },
            new() { Id = "t2", Duration = TimeSpan.FromMinutes(2) },
        };

        _hub.SetTrackList(tracks);

        _hub.Tracks.Should().BeEquivalentTo(tracks);
        _hub.Queue.Should().BeEquivalentTo(tracks);
    }

    [Fact]
    public void BanTrack_AddsTrackToBanList()
    {
        var track = new Track { Id = "t1", Duration = TimeSpan.FromMinutes(3) };
        _hub.SetTrackList([track]);

        _hub.BanTrack(track);

        _hub.BannedTracks.Should().Contain(track);
    }

    [Fact]
    public void BanTrack_CalledTwice_DoesNotDuplicate()
    {
        var track = new Track { Id = "t1", Duration = TimeSpan.FromMinutes(3) };
        _hub.SetTrackList([track]);
        _hub.BanTrack(track);
        _hub.BanTrack(track);

        _hub.BannedTracks.Should().ContainSingle(t => t.Id == "t1");
    }

    [Fact]
    public void UnbanTrack_RemovesTrackFromBanList()
    {
        var track = new Track { Id = "t1", Duration = TimeSpan.FromMinutes(3) };
        _hub.SetTrackList([track]);
        _hub.BanTrack(track);

        _hub.UnbanTrack(track);

        _hub.BannedTracks.Should().BeEmpty();
    }

    // ── CanSuggest ────────────────────────────────────────────────────

    [Fact]
    public void CanSuggest_WithEligibleQueueTrack_ReturnsTrue()
    {
        _hub.SetTrackList([new Track { Id = "t1", Duration = TimeSpan.FromSeconds(90) }]);

        _hub.CanSuggest.Should().BeTrue();
    }

    [Fact]
    public void CanSuggest_NoEligibleTracks_ReturnsFalse()
    {
        _hub.SetTrackList([new Track { Id = "t1", Duration = TimeSpan.FromSeconds(30) }]);

        _hub.CanSuggest.Should().BeFalse();
    }

    [Fact]
    public void CanSuggest_EmptyQueue_ReturnsFalse()
    {
        _hub.CanSuggest.Should().BeFalse();
    }

    // ── SuggestTrack guards ───────────────────────────────────────────

    [Fact]
    public void SuggestTrack_ExceedsMaxDuration_Throws()
    {
        _hub.SetTrackList([new Track { Id = "t1", Duration = TimeSpan.FromSeconds(90) }]);
        var tooLong = new Track { Id = "long", Duration = Track.MaxDuration.Add(TimeSpan.FromSeconds(1)) };

        var act = () => _hub.SuggestTrack(tooLong);

        act.Should().Throw<InvalidOperationException>().WithMessage("*maximum*");
    }

    [Fact]
    public void SuggestTrack_BannedTrack_Throws()
    {
        var eligible = new Track { Id = "t1", Duration = TimeSpan.FromSeconds(90) };
        var banned = new Track { Id = "banned", Duration = TimeSpan.FromMinutes(2) };
        _hub.SetTrackList([eligible, banned]);
        _hub.BanTrack(banned);

        var act = () => _hub.SuggestTrack(banned);

        act.Should().Throw<InvalidOperationException>().WithMessage("*banned*");
    }

    [Fact]
    public void SuggestTrack_CanSuggestFalse_Throws()
    {
        // No tracks set → CanSuggest = false
        var track = new Track { Id = "t1", Duration = TimeSpan.FromMinutes(2) };

        var act = () => _hub.SuggestTrack(track);

        act.Should().Throw<InvalidOperationException>().WithMessage("*eligible*");
    }

    [Fact]
    public void SuggestTrack_ValidTrack_AddedToSuggestedTracks()
    {
        _hub.SetTrackList([new Track { Id = "t1", Duration = TimeSpan.FromSeconds(90) }]);
        var suggested = new Track { Id = "s1", Duration = TimeSpan.FromMinutes(2) };

        _hub.SuggestTrack(suggested);

        _hub.SuggestedTracks.Should().Contain(suggested);
    }

    [Fact]
    public void SuggestTrack_DuplicateSuggestion_IgnoredSilently()
    {
        _hub.SetTrackList([new Track { Id = "t1", Duration = TimeSpan.FromSeconds(90) }]);
        var suggested = new Track { Id = "s1", Duration = TimeSpan.FromMinutes(2) };

        _hub.SuggestTrack(suggested);
        _hub.SuggestTrack(suggested); // second call silently ignored

        _hub.SuggestedTracks.Should().ContainSingle(t => t.Id == "s1");
    }

    // ── Playback ──────────────────────────────────────────────────────

    [Fact]
    public void Play_WithQueuedTrack_StartsPlayback()
    {
        _hub.SetTrackList([new Track { Id = "t1", Duration = TimeSpan.FromMinutes(3) }]);

        _hub.Play();

        _hub.State.Should().Be(PlaybackState.Playing);
        _hub.PlayingTrack!.Id.Should().Be("t1");
    }

    [Fact]
    public void Play_EmptyQueue_StopsPlayback()
    {
        _hub.Play();

        _hub.State.Should().Be(PlaybackState.Stopped);
        _hub.PlayingTrack.Should().BeNull();
    }

    [Fact]
    public void Stop_ResetsPlaybackState()
    {
        _hub.SetTrackList([new Track { Id = "t1", Duration = TimeSpan.FromMinutes(3) }]);
        _hub.Play();

        _hub.Stop();

        _hub.State.Should().Be(PlaybackState.Stopped);
        _hub.PlayingTrack.Should().BeNull();
        _hub.PlayingTrackPosition.Should().Be(TimeSpan.Zero);
    }

    [Fact]
    public void Next_AdvancesToNextQueuedTrack()
    {
        _hub.SetTrackList([
            new Track { Id = "t1", Duration = TimeSpan.FromMinutes(3) },
            new Track { Id = "t2", Duration = TimeSpan.FromMinutes(3) },
        ]);
        _hub.Play();

        _hub.Next();

        _hub.PlayingTrack!.Id.Should().Be("t2");
    }

    [Fact]
    public void Shuffle_PreservesQueueCount()
    {
        _hub.SetTrackList([
            new Track { Id = "t1", Duration = TimeSpan.FromMinutes(3) },
            new Track { Id = "t2", Duration = TimeSpan.FromMinutes(3) },
            new Track { Id = "t3", Duration = TimeSpan.FromMinutes(3) },
        ]);

        _hub.Shuffle();

        _hub.Queue.Should().HaveCount(3);
    }

    [Fact]
    public void ClearQueue_EmptiesQueue()
    {
        _hub.SetTrackList([new Track { Id = "t1", Duration = TimeSpan.FromMinutes(3) }]);

        _hub.ClearQueue();

        _hub.Queue.Should().BeEmpty();
    }

    // ── Users ─────────────────────────────────────────────────────────

    [Fact]
    public void AttachUser_AddsUserToAttachedUsers()
    {
        var user = new User { Id = "u1" };

        _hub.AttachUser(user);

        _hub.AttachedUsers.Should().Contain(user);
    }

    [Fact]
    public void AttachUser_DuplicateUser_DoesNotDuplicate()
    {
        var user = new User { Id = "u1" };
        _hub.AttachUser(user);
        _hub.AttachUser(user);

        _hub.AttachedUsers.Should().ContainSingle(u => u.Id == "u1");
    }

    [Fact]
    public void DetachUser_RemovesUserFromAttachedUsers()
    {
        var user = new User { Id = "u1" };
        _hub.AttachUser(user);

        _hub.DetachUser(user);

        _hub.AttachedUsers.Should().BeEmpty();
    }

    // ── CastVote ──────────────────────────────────────────────────────

    [Fact]
    public void CastVote_NoActiveVoting_Throws()
    {
        var act = () => _hub.CastVote(isUpVote: true, user: new User { Id = "u1" });

        act.Should().Throw<InvalidOperationException>().WithMessage("*voting*");
    }

    [Fact]
    public void CastVote_OwnerDownvote_TriggersVeto()
    {
        // Set up hub so voting is triggered immediately (nothing playing)
        _hub.SetTrackList([new Track { Id = "t1", Duration = TimeSpan.FromSeconds(90) }]);
        var suggested = new Track { Id = "s1", Duration = TimeSpan.FromMinutes(2) };
        _hub.SuggestTrack(suggested); // triggers voting immediately (hub is stopped)

        _hub.CastVote(isUpVote: false, user: new User { Id = _hub.Owner.Id });

        // Veto → voting ended, CurrentVoting removed, suggestion discarded
        _hub.CurrentVoting.Should().BeNull();
        _hub.SuggestedTracks.Should().BeEmpty();
    }
}
