using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.DeleteList;

public sealed record DeleteListCommand(Guid ListId) : ICommand;
