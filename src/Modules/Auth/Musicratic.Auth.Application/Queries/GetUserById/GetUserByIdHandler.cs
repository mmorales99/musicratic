using Musicratic.Auth.Application.DTOs;
using Musicratic.Auth.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Queries.GetUserById;

public sealed class GetUserByIdHandler(
    IUserRepository userRepository) : IQueryHandler<GetUserByIdQuery, UserDto?>
{
    public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
    {
        var user = await userRepository.GetById(request.UserId, cancellationToken);

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
