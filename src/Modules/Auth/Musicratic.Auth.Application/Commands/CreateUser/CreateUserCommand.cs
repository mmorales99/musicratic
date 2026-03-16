using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Commands.CreateUser;

public sealed record CreateUserCommand(
    string AuthentikSub,
    string DisplayName,
    string Email,
    string? AvatarUrl) : ICommand<Guid>;
