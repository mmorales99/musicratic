using Musicratic.Auth.Application.DTOs;
using Musicratic.Auth.Application.Services;
using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Commands.RefreshToken;

public sealed class RefreshTokenHandler(
    ITokenService tokenService) : ICommandHandler<RefreshTokenCommand, TokenResponseDto>
{
    public async Task<TokenResponseDto> Handle(
        RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        return await tokenService.RefreshToken(request.RefreshToken, cancellationToken);
    }
}
