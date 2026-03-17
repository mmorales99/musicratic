using Musicratic.Shared.Application;
using Musicratic.Shared.Contracts;

namespace Musicratic.Social.Application.Queries.GetUserProfile;

public sealed class GetUserProfileQueryHandler(
    IUserProfileProvider userProfileProvider) : IQueryHandler<GetUserProfileQuery, UserPublicProfileDto?>
{
    public async Task<UserPublicProfileDto?> Handle(GetUserProfileQuery request, CancellationToken ct)
    {
        var profile = await userProfileProvider.GetPublicProfile(request.UserId, ct);
        if (profile is null)
            return null;

        return new UserPublicProfileDto(
            UserId: profile.UserId,
            DisplayName: profile.DisplayName,
            AvatarUrl: profile.AvatarUrl,
            TotalProposals: profile.TotalProposals,
            TotalVotes: profile.TotalVotes,
            HubsVisited: profile.HubsVisited,
            MemberSince: profile.MemberSince);
    }
}
