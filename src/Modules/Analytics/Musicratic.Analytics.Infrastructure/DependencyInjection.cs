using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Analytics.Domain.Repositories;
using Musicratic.Analytics.Infrastructure.Persistence;
using Musicratic.Analytics.Application;

namespace Musicratic.Analytics.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAnalyticsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ANLT-003: DbContext + UnitOfWork
        services.AddDbContext<AnalyticsDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("AnalyticsDb")));

        services.AddScoped<IAnalyticsUnitOfWork, AnalyticsUnitOfWork>();

        // ANLT-002: Repositories
        services.AddScoped<ITrackStatsRepository, TrackStatsRepository>();
        services.AddScoped<IHubStatsRepository, HubStatsRepository>();

        // ANLT-010: MediatR handlers for Dapr event subscriptions
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
