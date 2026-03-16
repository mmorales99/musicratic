using Microsoft.Extensions.DependencyInjection;

namespace Musicratic.Hub.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddHubApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
