namespace Musicratic.Modules.MusicSessions.Application;

using Musicratic.Modules.MusicSessions.Domain;

public interface ISongQueueService
{
	SongQueue AddPendingSong(SongQueue queue, Guid songId, string title, string artist, string source);

	SongQueue RemovePendingSong(SongQueue queue, Guid songId);
}
