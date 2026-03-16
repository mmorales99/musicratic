using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.GenerateDeepLink;

public sealed record GenerateDeepLinkCommand(Guid HubId) : ICommand<string>;
