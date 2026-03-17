using Musicratic.Analytics.Application.DTOs;
using Musicratic.Analytics.Domain.Repositories;

namespace Musicratic.Analytics.Application.Services;

/// <summary>
/// ANLT-007: Generates monthly report of top 10 most-proposed tracks.
/// Reports tracks with high engagement that hub managers may want to add.
/// </summary>
public sealed class MonthlyPopularProposalsService(
    ITrackStatsRepository trackStatsRepository,
    IHubStatsRepository hubStatsRepository) : IMonthlyPopularProposalsService
{
    private const int TopProposalsCount = 10;

    public async Task<PopularProposalsReport> GenerateReport(
        Guid hubId, CancellationToken cancellationToken = default)
    {
        var topTracks = await trackStatsRepository.GetTopByHub(
            hubId, TopProposalsCount, cancellationToken);

        var proposals = topTracks
            .Select(t => new PopularProposalEntry(
                t.TrackId,
                t.Plays,
                t.Upvotes,
                Math.Round(t.CalculateScore(), 3)))
            .ToList();

        return new PopularProposalsReport(hubId, DateTime.UtcNow, proposals);
    }

    public async Task<IReadOnlyList<PopularProposalsReport>> GenerateAllHubReports(
        CancellationToken cancellationToken = default)
    {
        var activeHubs = await hubStatsRepository.GetActiveHubs(cancellationToken);
        var reports = new List<PopularProposalsReport>();

        foreach (var hub in activeHubs)
        {
            var report = await GenerateReport(hub.HubId, cancellationToken);
            if (report.Proposals.Count > 0)
                reports.Add(report);
        }

        return reports;
    }
}
