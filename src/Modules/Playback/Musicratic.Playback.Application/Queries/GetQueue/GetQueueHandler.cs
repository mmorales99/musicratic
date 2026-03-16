using Musicratic.Playback.Application.DTOs;
using Musicratic.Playback.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Queries.GetQueue;

public sealed class GetQueueHandler(
    IQueueEntryRepository queueEntryRepository,
    ITrackRepository trackRepository) : IQueryHandler<GetQueueQuery, GetQueueResult>
{
    public async Task<GetQueueResult> Handle(
        GetQueueQuery request, CancellationToken cancellationToken)
    {
        var skip = (request.Page - 1) * request.PageSize;

        var (entries, totalCount) = await queueEntryRepository.GetByHubIdPaginated(
            request.HubId, skip, request.PageSize, cancellationToken);

        var trackIds = entries.Select(e => e.TrackId).Distinct().ToList();
        var tracks = await trackRepository.Find(
            t => trackIds.Contains(t.Id), cancellationToken);
        var trackMap = tracks.ToDictionary(t => t.Id);

        var items = entries.Select(entry =>
        {
            trackMap.TryGetValue(entry.TrackId, out var track);
            return new QueueEntryDto(
                Id: entry.Id,
                TrackId: entry.TrackId,
                Position: entry.Position,
                Status: entry.Status.ToString(),
                Source: entry.Source.ToString(),
                ProposerId: entry.ProposerId,
                CostPaid: entry.CostPaid,
                Title: track?.Title ?? "Unknown",
                Artist: track?.Artist ?? "Unknown",
                Album: track?.Album,
                AlbumArtUrl: track?.AlbumArtUrl,
                DurationSeconds: track?.DurationSeconds ?? 0,
                StartedAt: entry.StartedAt,
                EndedAt: entry.EndedAt);
        }).ToList();

        var hasMore = skip + request.PageSize < totalCount;

        return new GetQueueResult(items, totalCount, hasMore);
    }
}
