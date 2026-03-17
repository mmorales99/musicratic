using Microsoft.Extensions.DependencyInjection;
using Musicratic.Analytics.Application.Services;

namespace Musicratic.Analytics.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalyticsApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // ANLT-004: Stats aggregation
        services.AddScoped<IStatsAggregationService, StatsAggregationService>();

        // ANLT-005: Shuffle weight calculation
        services.AddScoped<IShuffleWeightService, ShuffleWeightService>();

        // ANLT-006: Weekly downvoted tracks report
        services.AddScoped<IWeeklyDownvotedReportService, WeeklyDownvotedReportService>();

        // ANLT-007: Monthly popular proposals report
        services.AddScoped<IMonthlyPopularProposalsService, MonthlyPopularProposalsService>();

        // ANLT-008: Hotness calculation
        services.AddScoped<IHotnessService, HotnessService>();

        return services;
    }
}
