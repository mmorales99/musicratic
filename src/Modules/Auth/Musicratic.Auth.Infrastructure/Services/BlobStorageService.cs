using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musicratic.Auth.Application.Services;
using Musicratic.Auth.Infrastructure.Configuration;

namespace Musicratic.Auth.Infrastructure.Services;

public sealed class BlobStorageService : IBlobStorageService
{
    private readonly BlobStorageOptions _options;
    private readonly ILogger<BlobStorageService> _logger;

    public BlobStorageService(
        IOptions<BlobStorageOptions> options,
        ILogger<BlobStorageService> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> Upload(
        Stream stream,
        string containerName,
        string fileName,
        string contentType,
        CancellationToken cancellationToken)
    {
        var directoryPath = Path.Combine(_options.BasePath, containerName, Path.GetDirectoryName(fileName) ?? "");
        Directory.CreateDirectory(directoryPath);

        var filePath = Path.Combine(_options.BasePath, containerName, fileName);

        await using var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await stream.CopyToAsync(fileStream, cancellationToken);

        var url = $"{_options.BaseUrl.TrimEnd('/')}/{containerName}/{fileName}";

        _logger.LogInformation(
            "Uploaded blob {FileName} to {ContainerName}, URL: {Url}",
            fileName, containerName, url);

        return url;
    }
}
