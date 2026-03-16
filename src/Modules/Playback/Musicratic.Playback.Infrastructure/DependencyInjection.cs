using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Playback.Domain.Repositories;
using Musicratic.Playback.Infrastructure.Persistence;
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

        return services;
    }
}
