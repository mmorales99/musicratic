using Musicratic.Voting.Application.DTOs;

namespace Musicratic.Voting.Application.Services;

/// <summary>
/// VOTE-007: Evaluates skip rules per docs/05-voting-and-playback.md.
/// Formula: skip = d/(u+d) ≥ 0.65
/// VOTE-011: Minimum vote count threshold before skip activates.
/// </summary>
public interface ISkipRuleEngine
{
    SkipDecision Evaluate(int upvotes, int downvotes, int minimumVoteCount = 3);
}
