using Musicratic.Hub.Domain.Entities;
using Musicratic.Hub.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.CreateHub;

public sealed record CreateHubCommand(
    string Name,
    HubType Type,
    Guid OwnerId,
    HubSettings Settings) : ICommand<Guid>;
