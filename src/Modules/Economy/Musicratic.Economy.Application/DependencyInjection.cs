using Microsoft.Extensions.DependencyInjection;

namespace Musicratic.Economy.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddEconomyApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
