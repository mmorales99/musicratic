using Musicratic.Auth.Application.Services;
using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Commands.Logout;

public sealed class LogoutHandler(
    ITokenService tokenService) : ICommandHandler<LogoutCommand>
{
    public async Task Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        await tokenService.RevokeToken(request.RefreshToken, cancellationToken);
    }
}
