using Musicratic.Hub.Application.Services;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.GenerateHubQr;

public sealed class GenerateHubQrHandler(
    IHubRepository hubRepository,
    IHubLinkService hubLinkService,
    IQrCodeService qrCodeService,
    IUnitOfWork unitOfWork) : ICommandHandler<GenerateHubQrCommand, GenerateHubQrResult>
{
    public async Task<GenerateHubQrResult> Handle(
        GenerateHubQrCommand request,
        CancellationToken cancellationToken)
    {
        var hub = await hubRepository.GetById(request.HubId, cancellationToken)
            ?? throw new InvalidOperationException($"Hub '{request.HubId}' not found.");

        var joinUrl = hubLinkService.GenerateJoinUrl(hub.Code);
        var qrImageBytes = await qrCodeService.GenerateQrCodeAsync(joinUrl, cancellationToken: cancellationToken);

        hub.UpdateLinks(qrUrl: $"/qr/{hub.Code}", directLink: joinUrl);

        hubRepository.Update(hub);
        await unitOfWork.SaveChanges(cancellationToken);

        return new GenerateHubQrResult(joinUrl, qrImageBytes);
    }
}
