using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Playback.Application.Services;
using Musicratic.Playback.Domain.Repositories;
using Musicratic.Playback.Domain.Services;
using Musicratic.Playback.Infrastructure.Configuration;
using Musicratic.Playback.Infrastructure.Persistence;
using Musicratic.Playback.Infrastructure.Providers;
using Musicratic.Playback.Infrastructure.Services;
using Musicratic.Shared.Application;

namespace Musicratic.Playback.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddPlaybackInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<PlaybackDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("PlaybackDb")));

        services.AddScoped<ITrackRepository, TrackRepository>();
        services.AddScoped<IQueueEntryRepository, QueueEntryRepository>();
        services.AddScoped<IUnitOfWork, PlaybackUnitOfWork>();
        services.AddScoped<IPlaybackOrchestrator, PlaybackOrchestrator>();

        services.Configure<SpotifyOptions>(
            configuration.GetSection(SpotifyOptions.SectionName));
        services.Configure<YouTubeOptions>(
            configuration.GetSection(YouTubeOptions.SectionName));
        services.Configure<QueueInterleavingOptions>(
            configuration.GetSection(QueueInterleavingOptions.SectionName));

        services.AddHttpClient("SpotifyApi");
        services.AddHttpClient("SpotifyAuth");
        services.AddHttpClient("YouTubeApi");

        services.AddSingleton<IMusicProviderService, SpotifyProvider>();
        services.AddSingleton<IMusicProviderService, YouTubeMusicProvider>();

        services.AddScoped<IQueueInterleavingService, QueueInterleavingService>();
        services.AddScoped<ICollectiveVoteService, CollectiveVoteService>();
        services.AddScoped<IQueueBroadcastService, QueueBroadcastService>();

        services.AddSingleton<HubConnectionManager>();
        services.AddSingleton<IHubConnectionManager>(sp =>
            sp.GetRequiredService<HubConnectionManager>());

        return services;
    }
}
