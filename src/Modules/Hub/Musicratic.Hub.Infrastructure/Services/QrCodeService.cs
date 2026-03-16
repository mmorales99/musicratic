using Musicratic.Hub.Application.Services;
using QRCoder;

namespace Musicratic.Hub.Infrastructure.Services;

public sealed class QrCodeService : IQrCodeService
{
    public Task<byte[]> GenerateQrCodeAsync(
        string content,
        int size = 300,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(size, 0);

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.M);
        using var qrCode = new PngByteQRCode(qrCodeData);

        var pixelsPerModule = Math.Max(1, size / qrCodeData.ModuleMatrix.Count);
        var pngBytes = qrCode.GetGraphic(pixelsPerModule);

        return Task.FromResult(pngBytes);
    }
}
