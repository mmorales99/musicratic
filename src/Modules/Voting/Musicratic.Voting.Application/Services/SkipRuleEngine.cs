using Musicratic.Voting.Application.DTOs;

namespace Musicratic.Voting.Application.Services;

/// <summary>
/// VOTE-007 + VOTE-011: Evaluates skip rules.
/// Spec (docs/05-voting-and-playback.md): skip = d/(u+d) ≥ 0.65
/// Minimum vote count threshold must be met before skip activates.
/// </summary>
public sealed class SkipRuleEngine : ISkipRuleEngine
{
    private const double DefaultSkipPercentage = 65.0;

    public SkipDecision Evaluate(int upvotes, int downvotes, int minimumVoteCount = 3)
    {
        var totalVotes = upvotes + downvotes;

        if (totalVotes == 0)
            return new SkipDecision(false, "No votes cast.");

        // VOTE-011: If total votes < minimum, skip never triggers
        if (totalVotes < minimumVoteCount)
        {
            return new SkipDecision(
                false,
                $"Minimum vote count not met ({totalVotes}/{minimumVoteCount}).");
        }

        // Spec: skip = d/(u+d) ≥ 0.65
        var downvotePercentage = (double)downvotes / totalVotes * 100;

        if (downvotePercentage >= DefaultSkipPercentage)
        {
            return new SkipDecision(
                true,
                $"Downvote threshold reached: {downvotePercentage:F1}% >= {DefaultSkipPercentage}%.",
                downvotePercentage);
        }

        return new SkipDecision(false, DownvotePercentage: downvotePercentage);
    }
}
