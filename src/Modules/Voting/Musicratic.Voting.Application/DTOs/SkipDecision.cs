namespace Musicratic.Voting.Application.DTOs;

public sealed record SkipDecision(
    bool ShouldSkip,
    string? Reason = null,
    double DownvotePercentage = 0);
