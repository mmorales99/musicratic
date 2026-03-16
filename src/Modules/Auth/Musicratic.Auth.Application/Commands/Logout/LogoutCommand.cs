using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Commands.Logout;

public sealed record LogoutCommand(string RefreshToken) : ICommand;
