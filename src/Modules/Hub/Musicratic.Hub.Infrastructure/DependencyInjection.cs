using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Hub.Infrastructure.Persistence;
using Musicratic.Shared.Application;

namespace Musicratic.Hub.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddHubInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<HubDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("HubDb")));

        services.AddScoped<IHubRepository, HubRepository>();
        services.AddScoped<IHubMemberRepository, HubMemberRepository>();
        services.AddScoped<IHubAttachmentRepository, HubAttachmentRepository>();
        services.AddScoped<IListRepository, ListRepository>();
        services.AddScoped<IUnitOfWork, HubUnitOfWork>();

        return services;
    }
}
