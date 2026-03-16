using Musicratic.Shared.Application;

namespace Musicratic.Hub.Application.Commands.ReorderListTrack;

public sealed record ReorderListTrackCommand(Guid ListId, Guid TrackId, int NewPosition) : ICommand;
