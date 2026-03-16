using Musicratic.Playback.Domain.Entities;
using Musicratic.Playback.Domain.Enums;

namespace Musicratic.TestUtilities.Builders;

public sealed class QueueEntryBuilder
{
    private Guid _tenantId = Guid.NewGuid();
    private Guid _trackId = Guid.NewGuid();
    private Guid _hubId = Guid.NewGuid();
    private int _position;
    private QueueEntrySource _source = QueueEntrySource.List;
    private Guid? _proposerId;
    private int _costPaid;

    public QueueEntryBuilder WithTenantId(Guid tenantId)
    {
        _tenantId = tenantId;
        return this;
    }

    public QueueEntryBuilder WithTrackId(Guid trackId)
    {
        _trackId = trackId;
        return this;
    }

    public QueueEntryBuilder WithHubId(Guid hubId)
    {
        _hubId = hubId;
        return this;
    }

    public QueueEntryBuilder WithPosition(int position)
    {
        _position = position;
        return this;
    }

    public QueueEntryBuilder WithSource(QueueEntrySource source)
    {
        _source = source;
        return this;
    }

    public QueueEntryBuilder WithProposerId(Guid? proposerId)
    {
        _proposerId = proposerId;
        return this;
    }

    public QueueEntryBuilder WithCostPaid(int costPaid)
    {
        _costPaid = costPaid;
        return this;
    }

    public QueueEntry Build() => QueueEntry.Create(
        _tenantId, _trackId, _hubId, _position, _source, _proposerId, _costPaid);
}
