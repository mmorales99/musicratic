namespace Musicratic.Playback.Application.DTOs;

public sealed record CooldownCheckDto(
    bool CanPropose,
    string? Reason = null,
    DateTime? CooldownEndsAt = null);
