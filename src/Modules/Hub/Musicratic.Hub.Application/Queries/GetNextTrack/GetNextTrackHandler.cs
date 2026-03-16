using Musicratic.Hub.Application.DTOs;
using Musicratic.Hub.Application.Services;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Queries.GetNextTrack;

public sealed class GetNextTrackHandler(
    IListRepository listRepository,
    IPlayModeService playModeService) : IQueryHandler<GetNextTrackQuery, ListTrackDto?>
{
    public async Task<ListTrackDto?> Handle(
        GetNextTrackQuery request,
        CancellationToken cancellationToken)
    {
        var list = await listRepository.GetById(request.ListId, cancellationToken)
            ?? throw new InvalidOperationException($"List '{request.ListId}' not found.");

        var track = playModeService.GetNextTrack(list, request.CurrentTrackId);

        if (track is null)
            return null;

        return new ListTrackDto(
            track.Id,
            track.TrackId,
            track.Position,
            track.AddedAt,
            track.TotalUpvotes,
            track.TotalDownvotes,
            track.TotalPlays,
            track.ShuffleWeight);
    }
}
