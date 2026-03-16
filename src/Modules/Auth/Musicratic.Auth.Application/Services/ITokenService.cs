using Musicratic.Auth.Application.DTOs;

namespace Musicratic.Auth.Application.Services;

public interface ITokenService
{
    Task<TokenResponseDto> RefreshToken(
        string refreshToken, CancellationToken cancellationToken = default);

    Task RevokeToken(
        string token, CancellationToken cancellationToken = default);
}
