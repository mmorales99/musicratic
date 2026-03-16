using Musicratic.Hub.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.SetPlayMode;

public sealed record SetPlayModeCommand(Guid ListId, PlayMode PlayMode) : ICommand;
