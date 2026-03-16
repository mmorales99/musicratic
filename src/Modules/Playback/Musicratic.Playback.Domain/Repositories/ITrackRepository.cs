using Musicratic.Playback.Domain.Entities;
using Musicratic.Playback.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Playback.Domain.Repositories;

public interface ITrackRepository : IRepository<Track>
{
    Task<Track?> GetByExternalId(
        MusicProvider provider,
        string externalId,
        CancellationToken cancellationToken = default);
}
