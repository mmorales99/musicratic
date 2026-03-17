using Musicratic.Playback.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.ProposeTrackPaid;

public sealed record ProposeTrackPaidCommand(
    Guid TenantId,
    Guid HubId,
    Guid TrackId,
    Guid ProposerId,
    int CoinAmount) : ICommand<ProposalDto>;
