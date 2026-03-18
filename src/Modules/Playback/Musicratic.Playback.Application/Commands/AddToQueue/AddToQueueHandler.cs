using Musicratic.Playback.Application.DTOs;
using Musicratic.Playback.Domain.Entities;
using Musicratic.Playback.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.AddToQueue;

public sealed class AddToQueueHandler(
    IQueueEntryRepository queueEntryRepository,
    ITrackRepository trackRepository,
    IPlaybackUnitOfWork unitOfWork) : ICommandHandler<AddToQueueCommand, QueueEntryDto>
{
    public async Task<QueueEntryDto> Handle(
        AddToQueueCommand request, CancellationToken cancellationToken)
    {
        var track = await trackRepository.GetById(request.TrackId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"Track '{request.TrackId}' not found.");

        var nextPosition = await queueEntryRepository.GetNextPosition(
            request.HubId, cancellationToken);

        var entry = QueueEntry.Create(
            tenantId: request.TenantId,
            trackId: request.TrackId,
            hubId: request.HubId,
            position: nextPosition,
            source: request.Source,
            proposerId: request.ProposerId,
            costPaid: request.CostPaid);

        await queueEntryRepository.Add(entry, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        return new QueueEntryDto(
            Id: entry.Id,
            TrackId: track.Id,
            Position: entry.Position,
            Status: entry.Status.ToString(),
            Source: entry.Source.ToString(),
            ProposerId: entry.ProposerId,
            CostPaid: entry.CostPaid,
            Title: track.Title,
            Artist: track.Artist,
            Album: track.Album,
            AlbumArtUrl: track.AlbumArtUrl,
            DurationSeconds: track.DurationSeconds,
            StartedAt: entry.StartedAt,
            EndedAt: entry.EndedAt);
    }
}
