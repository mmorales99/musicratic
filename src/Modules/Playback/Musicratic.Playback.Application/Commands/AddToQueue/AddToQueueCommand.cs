using Musicratic.Playback.Application.DTOs;
using Musicratic.Playback.Domain.Enums;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.AddToQueue;

public sealed record AddToQueueCommand(
    Guid TenantId,
    Guid HubId,
    Guid TrackId,
    QueueEntrySource Source,
    Guid? ProposerId = null,
    int CostPaid = 0) : ICommand<QueueEntryDto>;
