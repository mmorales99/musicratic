using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.RemoveListTrack;

public sealed record RemoveListTrackCommand(Guid ListId, Guid TrackId) : ICommand;
