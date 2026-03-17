using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Analytics.Api.DTOs;
using Musicratic.Analytics.Application.Services;
using Musicratic.Analytics.Domain.Entities;
using Musicratic.Analytics.Domain.Repositories;

namespace Musicratic.Analytics.Api.Endpoints;

/// <summary>
/// ANLT-009: Analytics API endpoints with Problem Details (RFC 9457).
/// </summary>
public static class AnalyticsEndpoints
{
    public static RouteGroupBuilder MapAnalyticsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/analytics").WithTags("Analytics");

        group.MapGet("/hubs/{hubId:guid}/stats", GetHubStats)
            .WithName("GetHubStats");

        group.MapGet("/hubs/{hubId:guid}/tracks", GetHubTrackStats)
            .WithName("GetHubTrackStats");

        group.MapGet("/hubs/{hubId:guid}/top-tracks", GetTopTracks)
            .WithName("GetTopTracks");

        group.MapGet("/hubs/{hubId:guid}/reports/downvoted", GetDownvotedReport)
            .WithName("GetDownvotedReport");

        group.MapGet("/hubs/{hubId:guid}/reports/proposals", GetProposalsReport)
            .WithName("GetProposalsReport");

        group.MapGet("/tracks/{trackId:guid}/stats", GetTrackStats)
            .WithName("GetTrackStats");

        return group;
    }

    private static async Task<IResult> GetHubStats(
        Guid hubId,
        IHubStatsRepository hubStatsRepository,
        CancellationToken cancellationToken)
    {
        var stats = await hubStatsRepository.GetByHub(hubId, cancellationToken);

        if (stats is null)
        {
            return Results.Problem(
                title: "Hub stats not found",
                detail: $"No statistics found for hub {hubId}.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Results.Ok(MapToHubStatsResponse(stats));
    }

    private static async Task<IResult> GetHubTrackStats(
        Guid hubId,
        ITrackStatsRepository trackStatsRepository,
        int skip = 0,
        int take = 20,
        string sortBy = "plays",
        bool descending = true,
        CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 100);
        skip = Math.Max(skip, 0);

        var items = await trackStatsRepository.GetByHubPaged(
            hubId, skip, take, sortBy, descending, cancellationToken);

        var total = await trackStatsRepository.CountByHub(hubId, cancellationToken);

        var response = new TrackStatsPagedResponse(
            Success: true,
            TotalItemsInResponse: items.Count,
            HasMoreItems: skip + take < total,
            Items: items.Select(MapToTrackStatsResponse).ToList());

        return Results.Ok(response);
    }

    private static async Task<IResult> GetTopTracks(
        Guid hubId,
        ITrackStatsRepository trackStatsRepository,
        int count = 10,
        CancellationToken cancellationToken = default)
    {
        count = Math.Clamp(count, 1, 50);

        var items = await trackStatsRepository.GetTopByHub(
            hubId, count, cancellationToken);

        return Results.Ok(new
        {
            success = true,
            total_items_in_response = items.Count,
            has_more_items = false,
            items = items.Select(MapToTrackStatsResponse).ToList()
        });
    }

    private static async Task<IResult> GetDownvotedReport(
        Guid hubId,
        IWeeklyDownvotedReportService reportService,
        CancellationToken cancellationToken)
    {
        var report = await reportService.GenerateReport(hubId, cancellationToken);
        return Results.Ok(report);
    }

    private static async Task<IResult> GetProposalsReport(
        Guid hubId,
        IMonthlyPopularProposalsService reportService,
        CancellationToken cancellationToken)
    {
        var report = await reportService.GenerateReport(hubId, cancellationToken);
        return Results.Ok(report);
    }

    private static async Task<IResult> GetTrackStats(
        Guid trackId,
        ITrackStatsRepository trackStatsRepository,
        Guid? hubId = null,
        CancellationToken cancellationToken = default)
    {
        if (hubId.HasValue)
        {
            var stats = await trackStatsRepository.GetByTrackAndHub(
                trackId, hubId.Value, cancellationToken);

            if (stats is null)
            {
                return Results.Problem(
                    title: "Track stats not found",
                    detail: $"No statistics found for track {trackId} in hub {hubId}.",
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Ok(MapToTrackStatsResponse(stats));
        }

        var allStats = await trackStatsRepository.Find(
            t => t.TrackId == trackId, cancellationToken);

        if (allStats.Count == 0)
        {
            return Results.Problem(
                title: "Track stats not found",
                detail: $"No statistics found for track {trackId}.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Results.Ok(allStats.Select(MapToTrackStatsResponse).ToList());
    }

    private static HubStatsResponse MapToHubStatsResponse(HubStats stats)
    {
        return new HubStatsResponse(
            stats.HubId,
            stats.TotalPlays,
            stats.TotalVotes,
            stats.UniqueListeners,
            stats.ActiveHours,
            stats.PeakConcurrentUsers,
            stats.LastActivityAt);
    }

    private static TrackStatsResponse MapToTrackStatsResponse(TrackStats stats)
    {
        return new TrackStatsResponse(
            stats.TrackId,
            stats.HubId,
            stats.Upvotes,
            stats.Downvotes,
            stats.Plays,
            stats.Skips,
            stats.TotalPlayDuration.TotalSeconds,
            stats.LastPlayedAt,
            Math.Round(stats.CalculateScore(), 3));
    }
}
