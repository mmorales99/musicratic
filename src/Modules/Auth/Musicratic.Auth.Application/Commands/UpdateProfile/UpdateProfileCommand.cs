using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(
    Guid UserId,
    string DisplayName,
    string? AvatarUrl) : ICommand;
