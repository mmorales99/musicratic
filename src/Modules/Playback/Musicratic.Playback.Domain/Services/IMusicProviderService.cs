using Musicratic.Playback.Domain.Enums;
using Musicratic.Playback.Domain.Models;

namespace Musicratic.Playback.Domain.Services;

public interface IMusicProviderService
{
    MusicProvider Provider { get; }

    Task<IReadOnlyList<TrackSearchResult>> Search(
        string query,
        int limit = 20,
        CancellationToken cancellationToken = default);

    Task<TrackMetadata?> GetMetadata(
        string externalId,
        CancellationToken cancellationToken = default);

    Task<string?> GetPlaybackUrl(
        string externalId,
        CancellationToken cancellationToken = default);
}
