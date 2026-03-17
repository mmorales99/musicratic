using Musicratic.Analytics.Application.DTOs;
using Musicratic.Analytics.Domain.Repositories;

namespace Musicratic.Analytics.Application.Services;

/// <summary>
/// ANLT-006: Generates weekly report of tracks with >40% downvotes.
/// Threshold per docs/05-voting-and-playback.md.
/// </summary>
public sealed class WeeklyDownvotedReportService(
    ITrackStatsRepository trackStatsRepository,
    IHubStatsRepository hubStatsRepository) : IWeeklyDownvotedReportService
{
    private const double DownvoteThreshold = 0.40;
    private const int MinimumVotesForReport = 5;

    public async Task<DownvotedTracksReport> GenerateReport(
        Guid hubId, CancellationToken cancellationToken = default)
    {
        var allStats = await trackStatsRepository.GetByHub(hubId, cancellationToken);

        var downvotedTracks = allStats
            .Where(t =>
            {
                var totalVotes = t.Upvotes + t.Downvotes;
                return totalVotes >= MinimumVotesForReport
                       && t.DownvotePercentage() > DownvoteThreshold;
            })
            .OrderByDescending(t => t.DownvotePercentage())
            .Select(t => new DownvotedTrackEntry(
                t.TrackId,
                t.Upvotes,
                t.Downvotes,
                Math.Round(t.DownvotePercentage() * 100, 1),
                t.Plays))
            .ToList();

        return new DownvotedTracksReport(hubId, DateTime.UtcNow, downvotedTracks);
    }

    public async Task<IReadOnlyList<DownvotedTracksReport>> GenerateAllHubReports(
        CancellationToken cancellationToken = default)
    {
        var activeHubs = await hubStatsRepository.GetActiveHubs(cancellationToken);
        var reports = new List<DownvotedTracksReport>();

        foreach (var hub in activeHubs)
        {
            var report = await GenerateReport(hub.HubId, cancellationToken);
            if (report.Tracks.Count > 0)
                reports.Add(report);
        }

        return reports;
    }
}
