namespace Musicratic.Modules.MusicSessions.Api;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Modules.MusicSessions.Infrastructure;
using Musicratic.Shared.Contracts;

public static class MusicSessionsModule
{
	public static IEndpointRouteBuilder MapMusicSessionsModule(this IEndpointRouteBuilder endpoints)
	{
		var group = endpoints.MapGroup("/api/sessions");
		var runtimeGroup = group.MapGroup("/runtime");
		var songsGroup = group.MapGroup("/{sessionCode}/songs");

		group.MapGet("/{sessionCode}", (string sessionCode) => Results.Ok(SessionJoinPayloadDto.CreateEmpty(sessionCode)));

		songsGroup.MapGet("", () => Results.Ok(InMemorySongCatalog.GetAvailableSongs()));

		songsGroup.MapPost("/suggestions", (SongSuggestionRequestDto request) =>
		{
			var queue = InMemorySongQueueService.AddPendingSong(
				request.Song.Id,
				request.Song.Title,
				request.Song.Artist,
				request.Song.Source);

			var pendingSong = queue.PendingSongs[queue.PendingSongs.Count - 1];
			return Results.Ok(new PendingSongDto(
				pendingSong.Id,
				pendingSong.Title,
				pendingSong.Artist,
				pendingSong.Source,
				pendingSong.IsCandidate));
		});

		songsGroup.MapGet("/pending", () => Results.Ok(InMemorySongQueueService.CurrentQueue.PendingSongs.Select(song => new PendingSongDto(
			song.Id,
			song.Title,
			song.Artist,
			song.Source,
			song.IsCandidate))));

		runtimeGroup.MapGet("/states", () => Results.Ok(new[]
		{
			new SessionRuntimeStateInfoDto(SessionRuntimeState.Dead, "Dead", "Session is not running yet or has already been closed.", false),
			new SessionRuntimeStateInfoDto(SessionRuntimeState.Live, "Live", "Session is running and visitors can interact.", true)
		}));

		runtimeGroup.MapPost("/start", () => Results.Ok(Musicratic.Modules.MusicSessions.Domain.MusicSessionRuntime.Start()));

		runtimeGroup.MapPost("/close", () => Results.Ok(Musicratic.Modules.MusicSessions.Domain.MusicSessionRuntime.Close()));

		return endpoints;
	}
}
