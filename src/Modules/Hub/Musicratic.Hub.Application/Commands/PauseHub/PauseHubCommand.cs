using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.PauseHub;

public sealed record PauseHubCommand(Guid HubId) : ICommand;
