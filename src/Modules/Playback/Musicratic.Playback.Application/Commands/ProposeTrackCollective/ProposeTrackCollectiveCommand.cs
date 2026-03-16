using Musicratic.Playback.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.ProposeTrackCollective;

public sealed record ProposeTrackCollectiveCommand(
    Guid TenantId,
    Guid HubId,
    Guid TrackId,
    Guid ProposerId) : ICommand<ProposalDto>;
