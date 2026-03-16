using Musicratic.Hub.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.GetNextTrack;

public sealed record GetNextTrackQuery(Guid ListId, Guid? CurrentTrackId) : IQuery<ListTrackDto?>;
