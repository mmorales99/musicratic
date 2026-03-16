namespace Musicratic.Hub.Application.Services;

public interface IQrCodeService
{
    Task<byte[]> GenerateQrCodeAsync(string content, int size = 300, CancellationToken cancellationToken = default);
}
