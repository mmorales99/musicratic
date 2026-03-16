using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Hub.Application.Services;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Hub.Domain.Services;
using Musicratic.Hub.Infrastructure.Persistence;
using Musicratic.Hub.Infrastructure.Services;
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
        services.AddScoped<IHubCodeGenerator, HubCodeGenerator>();
        services.AddSingleton<IHubLinkService>(sp =>
            new HubLinkService(configuration));
        services.AddSingleton<IQrCodeService, QrCodeService>();

        return services;
    }
}
