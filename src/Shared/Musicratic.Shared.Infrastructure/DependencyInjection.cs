using Microsoft.Extensions.DependencyInjection;

namespace Musicratic.Shared.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSharedInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<TenantContext>();

        return services;
    }
}
