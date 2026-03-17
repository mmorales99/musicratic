using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Social.Application.Services;
using Musicratic.Social.Domain.Repositories;
using Musicratic.Social.Infrastructure.Persistence;
using Musicratic.Social.Infrastructure.Persistence.Repositories;
using Musicratic.Social.Infrastructure.Services;
using Musicratic.Shared.Application;

namespace Musicratic.Social.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddSocialInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<SocialDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("SocialDb")));

        services.AddScoped<IHubReviewRepository, HubReviewRepository>();
        services.AddScoped<IUnitOfWork, SocialUnitOfWork>();
        services.AddSingleton<ISocialSharingService, SocialSharingService>();
        services.AddScoped<IReviewEventPublisher, ReviewEventPublisher>();

        return services;
    }
}
