using Musicratic.Shared.Domain;

namespace Musicratic.Analytics.Domain.Entities;

/// <summary>
/// ANLT-001: Aggregated track statistics per hub.
/// Tracks upvotes, downvotes, plays, skips, and play duration.
/// </summary>
public sealed class TrackStats : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; private set; }

    public Guid TrackId { get; private set; }

    public Guid HubId { get; private set; }

    public int Upvotes { get; private set; }

    public int Downvotes { get; private set; }

    public int Plays { get; private set; }

    public int Skips { get; private set; }

    public TimeSpan TotalPlayDuration { get; private set; }

    public DateTime? LastPlayedAt { get; private set; }

    private TrackStats() { }

    public static TrackStats Create(Guid trackId, Guid hubId, Guid tenantId)
    {
        if (trackId == Guid.Empty)
            throw new ArgumentException("Track ID is required.", nameof(trackId));

        if (hubId == Guid.Empty)
            throw new ArgumentException("Hub ID is required.", nameof(hubId));

        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID is required.", nameof(tenantId));

        return new TrackStats
        {
            TrackId = trackId,
            HubId = hubId,
            TenantId = tenantId,
            Upvotes = 0,
            Downvotes = 0,
            Plays = 0,
            Skips = 0,
            TotalPlayDuration = TimeSpan.Zero
        };
    }

    public void IncrementUpvotes()
    {
        Upvotes++;
    }

    public void IncrementDownvotes()
    {
        Downvotes++;
    }

    public void IncrementPlays(TimeSpan duration)
    {
        Plays++;
        TotalPlayDuration += duration;
        LastPlayedAt = DateTime.UtcNow;
    }

    public void IncrementSkips()
    {
        Skips++;
    }

    /// <summary>
    /// Net score: upvotes minus downvotes, normalized by plays.
    /// Used for shuffle weight calculation (ANLT-005).
    /// </summary>
    public double CalculateScore()
    {
        var divisor = Math.Max(Plays, 1);
        return (double)(Upvotes - Downvotes) / divisor;
    }

    /// <summary>
    /// Downvote percentage: downvotes / total votes.
    /// Used for weekly downvoted tracks report (ANLT-006).
    /// </summary>
    public double DownvotePercentage()
    {
        var totalVotes = Upvotes + Downvotes;
        return totalVotes == 0 ? 0.0 : (double)Downvotes / totalVotes;
    }
}
