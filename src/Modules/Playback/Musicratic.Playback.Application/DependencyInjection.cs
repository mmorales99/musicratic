using Microsoft.Extensions.DependencyInjection;

namespace Musicratic.Playback.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddPlaybackApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
