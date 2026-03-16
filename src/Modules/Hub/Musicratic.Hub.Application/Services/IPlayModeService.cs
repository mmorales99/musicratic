using Musicratic.Hub.Domain.Entities;

namespace Musicratic.Hub.Application.Services;

public interface IPlayModeService
{
    ListTrack? GetNextTrack(List list, Guid? currentTrackId);
}
