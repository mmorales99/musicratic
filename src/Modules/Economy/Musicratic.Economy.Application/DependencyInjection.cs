using Microsoft.Extensions.DependencyInjection;
using Musicratic.Economy.Application.Services;

namespace Musicratic.Economy.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddEconomyApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // ECON-013: Subscription enforcement
        services.AddScoped<ISubscriptionEnforcementService, SubscriptionEnforcementService>();

        // ECON-014: Free trial service
        services.AddScoped<IFreeTrialService, FreeTrialService>();

        return services;
    }
}
