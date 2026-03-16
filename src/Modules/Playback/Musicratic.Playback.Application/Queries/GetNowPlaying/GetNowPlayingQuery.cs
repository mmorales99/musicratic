using Musicratic.Playback.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Queries.GetNowPlaying;

public sealed record GetNowPlayingQuery(Guid HubId) : IQuery<NowPlayingDto?>;
