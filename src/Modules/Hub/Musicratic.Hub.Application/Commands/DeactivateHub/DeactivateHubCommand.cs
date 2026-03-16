using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.DeactivateHub;

public sealed record DeactivateHubCommand(Guid HubId) : ICommand;
