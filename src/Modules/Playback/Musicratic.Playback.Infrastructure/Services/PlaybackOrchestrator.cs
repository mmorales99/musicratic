using Microsoft.Extensions.Logging;
using Musicratic.Playback.Application;
using Musicratic.Playback.Application.Services;
using Musicratic.Playback.Domain.Repositories;

namespace Musicratic.Playback.Infrastructure.Services;

public sealed class PlaybackOrchestrator(
    IQueueEntryRepository queueEntryRepository,
    IPlaybackUnitOfWork unitOfWork,
    ILogger<PlaybackOrchestrator> logger) : IPlaybackOrchestrator
{
    public async Task StartPlayback(Guid hubId, CancellationToken cancellationToken)
    {
        var currentlyPlaying = await queueEntryRepository.GetCurrentlyPlaying(
            hubId, cancellationToken);

        if (currentlyPlaying is not null)
        {
            throw new InvalidOperationException(
                $"Hub '{hubId}' already has a track playing (entry '{currentlyPlaying.Id}').");
        }

        var nextEntry = await queueEntryRepository.GetNextQueued(hubId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"No queued tracks available for hub '{hubId}'.");

        nextEntry.Play();
        queueEntryRepository.Update(nextEntry);
        await unitOfWork.SaveChanges(cancellationToken);

        logger.LogInformation(
            "Started playback for hub {HubId}: entry {QueueEntryId}, track {TrackId}",
            hubId, nextEntry.Id, nextEntry.TrackId);
    }

    public async Task AdvanceToNext(Guid hubId, CancellationToken cancellationToken)
    {
        await CompleteCurrentTrack(hubId, cancellationToken);
        await PlayNextIfAvailable(hubId, cancellationToken);
    }

    public async Task HandleTrackEnd(Guid hubId, CancellationToken cancellationToken)
    {
        // Same behavior as AdvanceToNext — triggered by track duration expiry.
        // Weighted shuffle integration with Hub module's PlayModeService
        // will be added when cross-module Dapr communication is wired up.
        await CompleteCurrentTrack(hubId, cancellationToken);
        await PlayNextIfAvailable(hubId, cancellationToken);
    }

    public async Task SkipCurrent(Guid hubId, CancellationToken cancellationToken)
    {
        var currentEntry = await queueEntryRepository.GetCurrentlyPlaying(
            hubId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"No track currently playing in hub '{hubId}'.");

        currentEntry.Skip();
        queueEntryRepository.Update(currentEntry);
        await unitOfWork.SaveChanges(cancellationToken);

        logger.LogInformation(
            "Skipped track in hub {HubId}: entry {QueueEntryId}, track {TrackId}",
            hubId, currentEntry.Id, currentEntry.TrackId);

        await PlayNextIfAvailable(hubId, cancellationToken);
    }

    private async Task CompleteCurrentTrack(Guid hubId, CancellationToken cancellationToken)
    {
        var currentEntry = await queueEntryRepository.GetCurrentlyPlaying(
            hubId, cancellationToken)
            ?? throw new InvalidOperationException(
                $"No track currently playing in hub '{hubId}'.");

        currentEntry.Complete();
        queueEntryRepository.Update(currentEntry);
        await unitOfWork.SaveChanges(cancellationToken);

        logger.LogInformation(
            "Completed track in hub {HubId}: entry {QueueEntryId}, track {TrackId}",
            hubId, currentEntry.Id, currentEntry.TrackId);
    }

    private async Task PlayNextIfAvailable(Guid hubId, CancellationToken cancellationToken)
    {
        var nextEntry = await queueEntryRepository.GetNextQueued(hubId, cancellationToken);

        if (nextEntry is null)
        {
            logger.LogInformation(
                "No more queued tracks for hub {HubId}. Playback idle.", hubId);
            return;
        }

        nextEntry.Play();
        queueEntryRepository.Update(nextEntry);
        await unitOfWork.SaveChanges(cancellationToken);

        logger.LogInformation(
            "Advanced to next track in hub {HubId}: entry {QueueEntryId}, track {TrackId}",
            hubId, nextEntry.Id, nextEntry.TrackId);
    }
}
