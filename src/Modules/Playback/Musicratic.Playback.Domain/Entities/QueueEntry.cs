using Musicratic.Playback.Domain.Enums;
using Musicratic.Playback.Domain.Events;
using Musicratic.Shared.Domain;

namespace Musicratic.Playback.Domain.Entities;

public sealed class QueueEntry : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; private set; }

    public Guid TrackId { get; private set; }

    public Guid HubId { get; private set; }

    public int Position { get; private set; }

    public QueueEntryStatus Status { get; private set; } = QueueEntryStatus.Queued;

    public QueueEntrySource Source { get; private set; }

    public Guid? ProposerId { get; private set; }

    public int CostPaid { get; private set; }

    public DateTime? StartedAt { get; private set; }

    public DateTime? EndedAt { get; private set; }

    private QueueEntry() { }

    public static QueueEntry Create(
        Guid tenantId,
        Guid trackId,
        Guid hubId,
        int position,
        QueueEntrySource source,
        Guid? proposerId = null,
        int costPaid = 0)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(position, 0, nameof(position));
        ArgumentOutOfRangeException.ThrowIfNegative(costPaid, nameof(costPaid));

        var entry = new QueueEntry
        {
            TenantId = tenantId,
            TrackId = trackId,
            HubId = hubId,
            Position = position,
            Source = source,
            ProposerId = proposerId,
            CostPaid = costPaid
        };

        entry.AddDomainEvent(new QueueEntryCreatedEvent(entry.Id, trackId, hubId));

        return entry;
    }

    public void Play()
    {
        if (Status is QueueEntryStatus.Played or QueueEntryStatus.Skipped)
        {
            throw new InvalidOperationException(
                $"Cannot play queue entry in '{Status}' status.");
        }

        Status = QueueEntryStatus.Playing;
        StartedAt = DateTime.UtcNow;

        AddDomainEvent(new TrackStartedPlayingEvent(Id, TrackId, HubId));
    }

    public void Complete()
    {
        if (Status != QueueEntryStatus.Playing)
        {
            throw new InvalidOperationException(
                $"Cannot complete queue entry in '{Status}' status. Must be Playing.");
        }

        Status = QueueEntryStatus.Played;
        EndedAt = DateTime.UtcNow;

        AddDomainEvent(new TrackCompletedEvent(Id, TrackId, HubId));
    }

    public void Skip()
    {
        if (Status is QueueEntryStatus.Played or QueueEntryStatus.Skipped)
        {
            throw new InvalidOperationException(
                $"Cannot skip queue entry in '{Status}' status.");
        }

        var wasPlaying = Status == QueueEntryStatus.Playing;

        Status = QueueEntryStatus.Skipped;
        EndedAt = DateTime.UtcNow;

        AddDomainEvent(new TrackSkippedEvent(Id, TrackId, HubId, wasPlaying));
    }
}
