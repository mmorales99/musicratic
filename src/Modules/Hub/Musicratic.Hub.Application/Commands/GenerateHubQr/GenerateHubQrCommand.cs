using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.GenerateHubQr;

public sealed record GenerateHubQrCommand(Guid HubId) : ICommand<GenerateHubQrResult>;

public sealed record GenerateHubQrResult(string JoinUrl, byte[] QrImageBytes);
