namespace Musicratic.Shared.Contracts;

public enum SessionRuntimeState
{
	Dead = 0,
	Live = 1
}

public enum JoinState
{
	UnverifiedGuest = 0,
	Guest = 1,
	PendingToJoin = 2,
	Joined = 3,
	Expired = 4
}

public enum VoteDirection
{
	Up = 0,
	Down = 1
}

public sealed record JoinStateInfoDto(JoinState State, string Name, string Description, bool CanSuggestSongs);

public sealed record AppAuthStatusDto(bool IsAuthenticated, bool CanSuggestSongs, string JoinState);

public sealed record SuggestionPermissionDto(bool CanSuggestSongs, JoinState JoinState, bool IsAuthenticated);

public sealed record SessionRuntimeStateInfoDto(SessionRuntimeState State, string Name, string Description, bool IsRunning);

public sealed record SongCatalogItemDto(Guid Id, string Title, string Artist, string Source);

public sealed record PendingSongDto(Guid Id, string Title, string Artist, string Source, bool IsCandidate);

public sealed record SongSuggestionRequestDto(SongCatalogItemDto Song);

public sealed record CurrentSongDto(Guid Id, string Title, string Artist, TimeSpan Remaining);

public sealed record SongOptionDto(Guid Id, string Title, string Artist, string Source);

public sealed record VoteSnapshotDto(int UpVotes, int DownVotes, bool IsTied);

public sealed record SessionJoinPayloadDto(
	string SessionCode,
	SessionRuntimeState SessionState,
	CurrentSongDto? CurrentSong,
	IReadOnlyList<SongOptionDto> Playlist,
	IReadOnlyList<SongOptionDto> AvailableSongs,
	VoteSnapshotDto VoteState,
	bool CanSuggestSongs)
{
	public static SessionJoinPayloadDto CreateEmpty(string sessionCode)
	{
		return new SessionJoinPayloadDto(
			sessionCode,
			SessionRuntimeState.Dead,
			null,
			Array.Empty<SongOptionDto>(),
			Array.Empty<SongOptionDto>(),
			new VoteSnapshotDto(0, 0, false),
			false);
	}
}
