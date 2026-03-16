using Musicratic.Hub.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Hub.Domain.Entities;

public sealed class List : AuditableEntity, ITenantScoped
{
    public Guid HubId { get; private set; }

    public Guid TenantId { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public Guid OwnerId { get; private set; }

    public PlayMode PlayMode { get; private set; }

    private readonly List<ListTrack> _tracks = [];

    public IReadOnlyCollection<ListTrack> Tracks => _tracks.AsReadOnly();

    private List() { }

    public static List Create(Guid hubId, Guid tenantId, string name, Guid ownerId, PlayMode playMode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new List
        {
            HubId = hubId,
            TenantId = tenantId,
            Name = name,
            OwnerId = ownerId,
            PlayMode = playMode
        };
    }

    public ListTrack AddTrack(Guid trackId)
    {
        var nextPosition = _tracks.Count + 1;
        var track = ListTrack.Create(Id, TenantId, trackId, nextPosition);
        _tracks.Add(track);
        return track;
    }

    public void UpdateName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name = name;
    }

    public void SetPlayMode(PlayMode playMode)
    {
        PlayMode = playMode;
    }
}
