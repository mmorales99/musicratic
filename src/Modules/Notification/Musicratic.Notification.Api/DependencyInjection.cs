using Microsoft.Extensions.DependencyInjection;

namespace Musicratic.Notification.Api;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationApi(this IServiceCollection services)
    {
        return services;
    }
}
