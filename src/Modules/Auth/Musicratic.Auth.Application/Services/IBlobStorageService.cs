namespace Musicratic.Auth.Application.Services;

public interface IBlobStorageService
{
    Task<string> Upload(
        Stream stream,
        string containerName,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
