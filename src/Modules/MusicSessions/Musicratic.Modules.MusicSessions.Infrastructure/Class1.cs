namespace Musicratic.Modules.MusicSessions.Infrastructure;

using Musicratic.Modules.MusicSessions.Domain;
using Musicratic.Shared.Contracts;

public static class InMemorySongQueueService
{
	private static SongQueue _queue = SongQueue.Empty;

	public static SongQueue CurrentQueue => _queue;

	public static SongQueue AddPendingSong(Guid songId, string title, string artist, string source)
	{
		_queue = _queue.AddPendingSong(songId, title, artist, source);
		return _queue;
	}

	public static SongQueue RemovePendingSong(Guid songId)
	{
		_queue = _queue.RemovePendingSong(songId);
		return _queue;
	}
}

public static class InMemorySongCatalog
{
	public static IReadOnlyList<SongCatalogItemDto> GetAvailableSongs()
	{
		return new[]
		{
			new SongCatalogItemDto(Guid.Parse("11111111-1111-1111-1111-111111111111"), "Sunset Drive", "Musicratic", "catalog"),
			new SongCatalogItemDto(Guid.Parse("22222222-2222-2222-2222-222222222222"), "Blue Hour", "Musicratic", "catalog"),
			new SongCatalogItemDto(Guid.Parse("33333333-3333-3333-3333-333333333333"), "Late Night Loop", "Musicratic", "catalog")
		};
	}
}
