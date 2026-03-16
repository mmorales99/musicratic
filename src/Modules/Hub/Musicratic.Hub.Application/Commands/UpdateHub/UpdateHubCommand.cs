using Musicratic.Hub.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.UpdateHub;

public sealed record UpdateHubCommand(
    Guid HubId,
    string Name,
    HubVisibility Visibility) : ICommand;
