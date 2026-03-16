using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.AttachUser;

public sealed record AttachUserCommand(string HubCode, Guid UserId) : ICommand<Guid>;
