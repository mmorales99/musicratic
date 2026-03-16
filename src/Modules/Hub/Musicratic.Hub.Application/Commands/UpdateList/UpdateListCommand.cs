using Musicratic.Hub.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.UpdateList;

public sealed record UpdateListCommand(
    Guid ListId,
    string Name,
    PlayMode PlayMode) : ICommand;
