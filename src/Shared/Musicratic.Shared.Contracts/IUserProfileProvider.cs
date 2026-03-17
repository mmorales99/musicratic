namespace Musicratic.Shared.Contracts;

public interface IUserProfileProvider
{
    Task<UserPublicProfile?> GetPublicProfile(Guid userId, CancellationToken ct);
}

public sealed record UserPublicProfile(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    int TotalProposals,
    int TotalVotes,
    int HubsVisited,
    DateTime MemberSince);
