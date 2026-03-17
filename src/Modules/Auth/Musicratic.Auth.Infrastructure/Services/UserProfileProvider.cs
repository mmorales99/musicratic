using Musicratic.Auth.Domain.Repositories;
using Musicratic.Shared.Contracts;

namespace Musicratic.Auth.Infrastructure.Services;

public sealed class UserProfileProvider(IUserRepository userRepository) : IUserProfileProvider
{
    public async Task<UserPublicProfile?> GetPublicProfile(Guid userId, CancellationToken ct)
    {
        var user = await userRepository.GetById(userId, ct);
        if (user is null)
            return null;

        // TotalProposals, TotalVotes, HubsVisited are placeholders — populated by analytics later
        return new UserPublicProfile(
            UserId: user.Id,
            DisplayName: user.DisplayName,
            AvatarUrl: user.AvatarUrl,
            TotalProposals: 0,
            TotalVotes: 0,
            HubsVisited: 0,
            MemberSince: user.CreatedAt);
    }
}
