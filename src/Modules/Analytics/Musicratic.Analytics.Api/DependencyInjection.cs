using Microsoft.Extensions.DependencyInjection;
using Musicratic.Analytics.Api.Services;

namespace Musicratic.Analytics.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalyticsApi(this IServiceCollection services)
    {
        // ANLT-006: Weekly report background service
        services.AddHostedService<WeeklyReportBackgroundService>();

        // ANLT-007: Monthly report background service
        services.AddHostedService<MonthlyReportBackgroundService>();

        return services;
    }
}
