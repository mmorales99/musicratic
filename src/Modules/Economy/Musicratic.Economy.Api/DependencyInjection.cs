using Microsoft.Extensions.DependencyInjection;
using Musicratic.Economy.Api.Services;

namespace Musicratic.Economy.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddEconomyApi(this IServiceCollection services)
    {
        // ECON-014: Trial expiry background service
        services.AddHostedService<TrialExpiryBackgroundService>();

        return services;
    }
}
