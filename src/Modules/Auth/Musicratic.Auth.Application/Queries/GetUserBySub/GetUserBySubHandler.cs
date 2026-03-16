using Musicratic.Auth.Application.DTOs;
using Musicratic.Auth.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Queries.GetUserBySub;

public sealed class GetUserBySubHandler(
    IUserRepository userRepository) : IQueryHandler<GetUserBySubQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetUserBySubQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetByAuthentikSub(request.AuthentikSub, cancellationToken);

        if (user is null)
            return null;

        return new UserDto(
            user.Id,
            user.DisplayName,
            user.Email,
            user.AvatarUrl,
            user.WalletBalance,
            user.CreatedAt);
    }
}
