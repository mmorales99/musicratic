namespace Musicratic.Voting.Application.Services;

/// <summary>
/// VOTE-008: Determines if a user has owner-level skip power in a hub.
/// Spec: "Owner downvote = instant skip (for tracks in assigned lists)" — docs/07-user-roles.md.
/// </summary>
public interface IOwnerVoteService
{
    Task<bool> IsOwnerWithSkipPower(Guid userId, Guid hubId, CancellationToken ct);
}
