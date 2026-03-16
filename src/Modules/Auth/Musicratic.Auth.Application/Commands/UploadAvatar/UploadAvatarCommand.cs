using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Commands.UploadAvatar;

public sealed record UploadAvatarCommand(
    Guid UserId,
    Stream FileStream,
    string FileName,
    string ContentType,
    long FileSize) : ICommand<string>;
