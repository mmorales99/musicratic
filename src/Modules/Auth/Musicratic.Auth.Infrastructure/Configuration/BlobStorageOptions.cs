namespace Musicratic.Auth.Infrastructure.Configuration;

public sealed class BlobStorageOptions
{
    public const string SectionName = "BlobStorage";

    public string BasePath { get; set; } = "blobs";

    public string BaseUrl { get; set; } = "/blobs";
}
