using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.DeleteHub;

public sealed record DeleteHubCommand(Guid HubId) : ICommand;
