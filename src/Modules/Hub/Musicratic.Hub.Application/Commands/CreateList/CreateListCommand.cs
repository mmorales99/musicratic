using Musicratic.Hub.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.CreateList;

public sealed record CreateListCommand(
    Guid HubId,
    string Name,
    Guid OwnerId,
    PlayMode PlayMode) : ICommand<Guid>;
