using Musicratic.Notification.Domain.Entities;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Notification.Application.Commands.RegisterDevice;

public sealed class RegisterDeviceHandler(
    IDeviceTokenRepository deviceTokenRepository,
    INotificationUnitOfWork unitOfWork) : ICommandHandler<RegisterDeviceCommand, Guid>
{
    public async Task<Guid> Handle(
        RegisterDeviceCommand request,
        CancellationToken cancellationToken)
    {
        var existing = await deviceTokenRepository.GetByUserIdAndToken(
            request.UserId,
            request.Token,
            cancellationToken);

        if (existing is not null)
        {
            existing.Update(request.DeviceName);
            existing.Activate();
            deviceTokenRepository.Update(existing);
            await unitOfWork.SaveChanges(cancellationToken);
            return existing.Id;
        }

        var deviceToken = DeviceToken.Create(
            request.UserId,
            request.Token,
            request.Platform,
            request.DeviceName);

        await deviceTokenRepository.Add(deviceToken, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        return deviceToken.Id;
    }
}
