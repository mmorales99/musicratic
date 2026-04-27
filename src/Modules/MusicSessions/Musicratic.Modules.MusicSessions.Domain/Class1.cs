namespace Musicratic.Modules.MusicSessions.Domain;

public enum SessionRuntimeState
{
	Dead = 0,
	Live = 1
}

public enum VoteDirection
{
	Up = 0,
	Down = 1
}

public enum TieBreakMode
{
	DownvotePriority = 0,
	UpvotePriority = 1,
	ShopOwnerRequired = 2
}

public sealed record MusicSessionRuntime(SessionRuntimeState State, string Description)
{
	public static MusicSessionRuntime Dead => new(SessionRuntimeState.Dead, "Session is not running yet or has already been closed.");

	public static MusicSessionRuntime Live => new(SessionRuntimeState.Live, "Session is running and visitors can interact.");

	public static MusicSessionRuntime Start()
	{
		return Live;
	}

	public static MusicSessionRuntime Close()
	{
		return Dead;
	}
}

public sealed record SongQueue(IReadOnlyList<QueuedSong> PendingSongs)
{
	public static SongQueue Empty => new(Array.Empty<QueuedSong>());

	public SongQueue AddPendingSong(Guid songId, string title, string artist, string source)
	{
		var pendingSongs = PendingSongs.ToList();
		pendingSongs.Add(new QueuedSong(songId, title, artist, source, false));
		return new SongQueue(pendingSongs);
	}

	public SongQueue RemovePendingSong(Guid songId)
	{
		var pendingSongs = PendingSongs.Where(song => song.Id != songId).ToList();
		return new SongQueue(pendingSongs);
	}
}

public sealed record QueuedSong(Guid Id, string Title, string Artist, string Source, bool IsCandidate);
