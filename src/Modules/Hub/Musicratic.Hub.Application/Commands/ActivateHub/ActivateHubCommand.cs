using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.ActivateHub;

public sealed record ActivateHubCommand(Guid HubId) : ICommand;
