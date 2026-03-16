namespace Musicratic.Voting.Application.DTOs;

public sealed record CooldownCheckResult(
    bool CanPropose,
    string? Reason = null,
    DateTime? CooldownEndsAt = null);
