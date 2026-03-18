using Musicratic.Auth.Application.Services;
using Musicratic.Auth.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Commands.UploadAvatar;

public sealed class UploadAvatarHandler(
    IUserRepository userRepository,
    IBlobStorageService blobStorageService,
    IAuthUnitOfWork unitOfWork) : ICommandHandler<UploadAvatarCommand, string>
{
    private const long MaxFileSizeBytes = 2 * 1024 * 1024; // 2 MB
    private static readonly HashSet<string> AllowedContentTypes = ["image/jpeg", "image/png"];
    private const string AvatarContainer = "avatars";

    public async Task<string> Handle(
        UploadAvatarCommand request, CancellationToken cancellationToken)
    {
        ValidateFile(request);

        var user = await userRepository.GetById(request.UserId, cancellationToken)
            ?? throw new InvalidOperationException($"User '{request.UserId}' not found.");

        var extension = GetFileExtension(request.ContentType);
        var blobFileName = $"{request.UserId}/{Guid.NewGuid()}{extension}";

        var avatarUrl = await blobStorageService.Upload(
            request.FileStream,
            AvatarContainer,
            blobFileName,
            request.ContentType,
            cancellationToken);

        user.UpdateProfile(user.DisplayName, avatarUrl);

        userRepository.Update(user);
        await unitOfWork.SaveChanges(cancellationToken);

        return avatarUrl;
    }

    private static void ValidateFile(UploadAvatarCommand request)
    {
        if (request.FileSize <= 0)
            throw new ArgumentException("File is empty.");

        if (request.FileSize > MaxFileSizeBytes)
            throw new ArgumentException($"File size exceeds the maximum allowed size of {MaxFileSizeBytes / (1024 * 1024)} MB.");

        if (!AllowedContentTypes.Contains(request.ContentType))
            throw new ArgumentException($"File type '{request.ContentType}' is not allowed. Allowed types: JPEG, PNG.");
    }

    private static string GetFileExtension(string contentType) => contentType switch
    {
        "image/jpeg" => ".jpg",
        "image/png" => ".png",
        _ => throw new ArgumentException($"Unsupported content type: {contentType}")
    };
}
