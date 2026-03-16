using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.ResumeHub;

public sealed record ResumeHubCommand(Guid HubId) : ICommand;
