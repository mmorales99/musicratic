using Musicratic.Auth.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Commands.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<TokenResponseDto>;
