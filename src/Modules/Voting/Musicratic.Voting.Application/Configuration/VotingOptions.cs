namespace Musicratic.Voting.Application.Configuration;

/// <summary>
/// Configurable voting options. Per docs/05-voting-and-playback.md.
/// Fallback chain: DB (hub settings) → appsettings → compile-time constants here.
/// </summary>
public sealed class VotingOptions
{
    public const string SectionName = "Voting";

    /// <summary>Voting window in seconds for proposed tracks (spec: 60s).</summary>
    public int DefaultVotingWindowSeconds { get; set; } = 60;

    /// <summary>Downvote percentage threshold for auto-skip (spec: 65%).</summary>
    public double DownvoteSkipPercentage { get; set; } = 65.0;

    /// <summary>Minimum votes before skip rule activates (VOTE-011, default: 3).</summary>
    public int MinimumVoteCountForSkip { get; set; } = 3;

    /// <summary>Max vote changes (remove + recast) per user per track (VOTE-010).</summary>
    public int MaxVoteChangesPerTrack { get; set; } = 3;

    /// <summary>Max votes per user per minute across all tracks (VOTE-010).</summary>
    public int MaxVotesPerMinute { get; set; } = 20;
}
