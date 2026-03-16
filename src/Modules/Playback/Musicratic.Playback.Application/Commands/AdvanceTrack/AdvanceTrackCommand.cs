using Musicratic.Shared.Application;

namespace Musicratic.Playback.Application.Commands.AdvanceTrack;

public sealed record AdvanceTrackCommand(Guid HubId) : ICommand;
