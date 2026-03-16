using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.UnregisterDevice;

public sealed class UnregisterDeviceHandler(
    IDeviceTokenRepository deviceTokenRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UnregisterDeviceCommand>
{
    public async Task Handle(
        UnregisterDeviceCommand request,
        CancellationToken cancellationToken)
    {
        var deviceToken = await deviceTokenRepository.GetById(request.TokenId, cancellationToken);

        if (deviceToken is null || deviceToken.UserId != request.UserId)
            return;

        deviceToken.Deactivate();
        deviceTokenRepository.Update(deviceToken);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
