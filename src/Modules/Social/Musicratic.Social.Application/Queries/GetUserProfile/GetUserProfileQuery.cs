using Musicratic.Shared.Application;

namespace Musicratic.Social.Application.Queries.GetUserProfile;

public sealed record GetUserProfileQuery(Guid UserId) : IQuery<UserPublicProfileDto?>;

public sealed record UserPublicProfileDto(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl,
    int TotalProposals,
    int TotalVotes,
    int HubsVisited,
    DateTime MemberSince);
