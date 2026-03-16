using Microsoft.Extensions.DependencyInjection;

namespace Musicratic.Social.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddSocialApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
