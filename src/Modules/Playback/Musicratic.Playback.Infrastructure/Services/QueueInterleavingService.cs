using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musicratic.Playback.Application.Services;
using Musicratic.Playback.Domain.Enums;
using Musicratic.Playback.Domain.Repositories;
using Musicratic.Playback.Infrastructure.Configuration;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Infrastructure.Services;

public sealed class QueueInterleavingService(
    IQueueEntryRepository queueEntryRepository,
    IUnitOfWork unitOfWork,
    IOptions<QueueInterleavingOptions> options,
    ILogger<QueueInterleavingService> logger) : IQueueInterleavingService
{
    private readonly int _ratio = options.Value.ListToProposalRatio;

    public async Task<int> CalculateInsertPosition(
        Guid hubId,
        QueueEntrySource source,
        CancellationToken cancellationToken = default)
    {
        if (source == QueueEntrySource.List)
        {
            return await queueEntryRepository.GetNextPosition(hubId, cancellationToken);
        }

        // For proposals: find the best interleaving slot.
        // Every _ratio list tracks, we insert 1 proposal.
        var entries = await queueEntryRepository.GetByHubId(hubId, cancellationToken);

        var queuedEntries = entries
            .Where(e => e.Status is QueueEntryStatus.Queued or QueueEntryStatus.Pending)
            .OrderBy(e => e.Position)
            .ToList();

        if (queuedEntries.Count == 0)
        {
            return await queueEntryRepository.GetNextPosition(hubId, cancellationToken);
        }

        // Count consecutive list tracks from the current position
        var listTrackCount = 0;
        var insertAfterPosition = queuedEntries.Last().Position;

        foreach (var entry in queuedEntries)
        {
            if (entry.Source == QueueEntrySource.List)
            {
                listTrackCount++;
                if (listTrackCount >= _ratio)
                {
                    insertAfterPosition = entry.Position;
                    break;
                }
            }
            else
            {
                listTrackCount = 0;
            }
        }

        logger.LogInformation(
            "Calculated interleave position {Position} for proposal in hub {HubId} (ratio: {Ratio})",
            insertAfterPosition + 1, hubId, _ratio);

        return insertAfterPosition + 1;
    }

    public async Task ReorderQueue(
        Guid hubId, CancellationToken cancellationToken = default)
    {
        var entries = await queueEntryRepository.GetByHubId(hubId, cancellationToken);

        var activeEntries = entries
            .Where(e => e.Status is QueueEntryStatus.Queued or QueueEntryStatus.Pending)
            .OrderBy(e => e.Position)
            .ToList();

        // Interleave: _ratio list tracks, then 1 proposal, repeat
        var listTracks = activeEntries
            .Where(e => e.Source == QueueEntrySource.List).ToList();
        var proposals = activeEntries
            .Where(e => e.Source != QueueEntrySource.List).ToList();

        var reordered = new List<Domain.Entities.QueueEntry>();
        var listIndex = 0;
        var proposalIndex = 0;
        var counter = 0;

        while (listIndex < listTracks.Count || proposalIndex < proposals.Count)
        {
            if (listIndex < listTracks.Count && (counter < _ratio || proposalIndex >= proposals.Count))
            {
                reordered.Add(listTracks[listIndex++]);
                counter++;
            }
            else if (proposalIndex < proposals.Count)
            {
                reordered.Add(proposals[proposalIndex++]);
                counter = 0;
            }
        }

        // Find the base position (after played/playing/skipped entries)
        var basePosition = entries
            .Where(e => e.Status is QueueEntryStatus.Playing or QueueEntryStatus.Played or QueueEntryStatus.Skipped)
            .Select(e => (int?)e.Position)
            .Max() ?? -1;

        for (var i = 0; i < reordered.Count; i++)
        {
            reordered[i].SetPosition(basePosition + 1 + i);
            queueEntryRepository.Update(reordered[i]);
        }

        await unitOfWork.SaveChanges(cancellationToken);

        logger.LogInformation(
            "Reordered {Count} queue entries in hub {HubId}", reordered.Count, hubId);
    }
}
