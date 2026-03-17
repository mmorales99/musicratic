using Musicratic.Analytics.Application.DTOs;

namespace Musicratic.Analytics.Application.Services;

/// <summary>
/// ANLT-007: Generates monthly report of top proposed tracks for hub managers.
/// </summary>
public interface IMonthlyPopularProposalsService
{
    Task<PopularProposalsReport> GenerateReport(
        Guid hubId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<PopularProposalsReport>> GenerateAllHubReports(
        CancellationToken cancellationToken = default);
}
