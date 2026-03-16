using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.DetachUser;

public sealed record DetachUserCommand(Guid UserId) : ICommand;
