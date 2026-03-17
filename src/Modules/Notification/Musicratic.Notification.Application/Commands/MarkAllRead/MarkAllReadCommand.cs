using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.MarkAllRead;

public sealed record MarkAllReadCommand(Guid UserId) : ICommand;
