using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Notification.Application.Services;
using Musicratic.Notification.Domain.Repositories;
using Musicratic.Notification.Infrastructure.Configuration;
using Musicratic.Notification.Infrastructure.EventHandlers;
using Musicratic.Notification.Infrastructure.Persistence;
using Musicratic.Notification.Infrastructure.Services;
using Musicratic.Notification.Application;

namespace Musicratic.Notification.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNotificationInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<NotificationDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("NotificationDb")));

        services.AddScoped<INotificationRepository, NotificationRepository>();
        services.AddScoped<IDeviceTokenRepository, DeviceTokenRepository>();
        services.AddScoped<INotificationPreferenceRepository, NotificationPreferenceRepository>();
        services.AddScoped<INotificationUnitOfWork, NotificationUnitOfWork>();

        services.AddSingleton<IConnectionManager, InMemoryConnectionManager>();
        services.AddSingleton<INotificationPushService, WebSocketNotificationService>();

        // Push notification config
        services.Configure<ApnsOptions>(configuration.GetSection(ApnsOptions.SectionName));
        services.Configure<FcmOptions>(configuration.GetSection(FcmOptions.SectionName));

        // Email notification config
        services.Configure<SmtpOptions>(configuration.GetSection(SmtpOptions.SectionName));
        services.AddScoped<IEmailNotificationService, EmailNotificationService>();

        services.AddHttpClient("ApnsPush");
        services.AddHttpClient("FcmPush");
        services.AddHttpClient("FcmAuth");

        services.AddSingleton<IApnsPushService, ApnsPushService>();
        services.AddSingleton<IFcmPushService, FcmPushService>();
        services.AddScoped<IPushNotificationService, PushNotificationService>();

        // NTFY-009: Dapr event handlers
        services.AddScoped<VoteCastNotificationHandler>();
        services.AddScoped<SkipTriggeredNotificationHandler>();
        services.AddScoped<TrackStartedNotificationHandler>();
        services.AddScoped<ReviewCreatedNotificationHandler>();
        services.AddScoped<ReportReadyNotificationHandler>();

        return services;
    }
}
