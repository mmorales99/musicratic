using FluentAssertions;
using Musicratic.Core.Models;

namespace Musicratic.Core.Tests;

[Collection("MusicraticSuite")]
public class VotingTests
{
    private static Track AnyTrack() => new() { Id = "t1", Duration = TimeSpan.FromMinutes(3) };

    [Fact]
    public void GetVoteResult_ZeroVotes_ReturnsPassed()
    {
        var v = new Voting { Track = AnyTrack() };
        v.GetVoteResult().Should().Be(VoteOutcome.Passed);
    }

    [Fact]
    public void GetVoteResult_AllUpvotes_ReturnsPassed()
    {
        var v = new Voting { Track = AnyTrack(), UpVotes = 5, DownVotes = 0 };
        v.GetVoteResult().Should().Be(VoteOutcome.Passed);
    }

    [Fact]
    public void GetVoteResult_Exactly65PercentDownvotes_ReturnsRejected()
    {
        // 13 down / 20 total = 65%
        var v = new Voting { Track = AnyTrack(), UpVotes = 7, DownVotes = 13 };
        v.GetVoteResult().Should().Be(VoteOutcome.Rejected);
    }

    [Fact]
    public void GetVoteResult_Below65PercentDownvotes_ReturnsPassed()
    {
        // 12 down / 20 total = 60%
        var v = new Voting { Track = AnyTrack(), UpVotes = 8, DownVotes = 12 };
        v.GetVoteResult().Should().Be(VoteOutcome.Passed);
    }

    [Fact]
    public void GetVoteResult_AllDownvotes_ReturnsRejected()
    {
        var v = new Voting { Track = AnyTrack(), UpVotes = 0, DownVotes = 5 };
        v.GetVoteResult().Should().Be(VoteOutcome.Rejected);
    }

    [Fact]
    public void CastVote_OwnerDownvote_ReturnsOwnerVeto()
    {
        var owner = new Owner { Id = "owner" };
        var v = new Voting { Track = AnyTrack() };
        VoteOutcome? result = null;
        v.StartVoting(voting => result = voting.GetVoteResult());

        v.CastVote(isUpVote: false, user: owner);

        result.Should().Be(VoteOutcome.OwnerVeto);
        v.IsActive.Should().BeFalse();
    }

    [Fact]
    public void CastVote_OwnerUpvote_CountsAsNormalVote_NoVeto()
    {
        var owner = new Owner { Id = "owner" };
        var v = new Voting { Track = AnyTrack() };
        bool completed = false;
        v.StartVoting(_ => completed = true);

        v.CastVote(isUpVote: true, user: owner);

        completed.Should().BeFalse("owner upvote should not trigger veto");
        v.UpVotes.Should().Be(1);
        v.IsActive.Should().BeTrue();
    }

    [Fact]
    public void CastVote_AfterForceComplete_ThrowsInvalidOperationException()
    {
        var v = new Voting { Track = AnyTrack() };
        v.StartVoting(_ => { });
        v.ForceComplete();

        var act = () => v.CastVote(isUpVote: true, user: new User { Id = "u1" });
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void ForceComplete_VotingBecomesInactive_CallbackFiredOnce()
    {
        var v = new Voting { Track = AnyTrack() };
        int calls = 0;
        v.StartVoting(_ => calls++);

        v.ForceComplete();
        v.ForceComplete(); // second call must be a no-op

        v.IsActive.Should().BeFalse();
        calls.Should().Be(1);
    }

    [Fact]
    public void IsActive_TrueBeforeComplete_FalseAfterComplete()
    {
        var v = new Voting { Track = AnyTrack() };
        v.StartVoting(_ => { });
        v.IsActive.Should().BeTrue();

        v.ForceComplete();
        v.IsActive.Should().BeFalse();
    }

    [Fact]
    public void TotalVotes_And_DownVoteRatio_ComputedCorrectly()
    {
        var v = new Voting { Track = AnyTrack(), UpVotes = 3, DownVotes = 7 };
        v.TotalVotes.Should().Be(10);
        v.DownVoteRatio.Should().BeApproximately(0.7, 0.001);
    }

    [Fact]
    public void DownVoteRatio_ZeroVotes_ReturnsZero()
    {
        var v = new Voting { Track = AnyTrack() };
        v.DownVoteRatio.Should().Be(0);
    }
}
