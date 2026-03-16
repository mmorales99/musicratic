using Musicratic.Playback.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Playback.Domain.Events;

public sealed record TrackCreatedEvent(
    Guid TrackId,
    MusicProvider Provider,
    string ExternalId) : DomainEvent;
