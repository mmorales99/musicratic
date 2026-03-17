using Musicratic.Analytics.Application.DTOs;

namespace Musicratic.Analytics.Application.Services;

/// <summary>
/// ANLT-006: Generates weekly report of tracks with >40% downvotes.
/// Publishes notification event for list owners.
/// </summary>
public interface IWeeklyDownvotedReportService
{
    Task<DownvotedTracksReport> GenerateReport(
        Guid hubId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DownvotedTracksReport>> GenerateAllHubReports(
        CancellationToken cancellationToken = default);
}
