using Musicratic.Hub.Application.Services;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.GenerateDeepLink;

public sealed class GenerateDeepLinkHandler(
    IHubRepository hubRepository,
    IHubLinkService hubLinkService,
    IUnitOfWork unitOfWork) : ICommandHandler<GenerateDeepLinkCommand, string>
{
    public async Task<string> Handle(
        GenerateDeepLinkCommand request,
        CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetById(request.HubId, cancellationToken)
            ?? throw new InvalidOperationException($"Hub '{request.HubId}' not found.");

        var deepLink = hubLinkService.GenerateJoinUrl(hub.Code);

        hub.UpdateLinks(hub.QrUrl, directLink: deepLink);

        hubRepository.Update(hub);
        await unitOfWork.SaveChanges(cancellationToken);

        return deepLink;
    }
}
