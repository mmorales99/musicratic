using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Hub.Application.Services;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Hub.Domain.Services;
using Musicratic.Hub.Infrastructure.Persistence;
using Musicratic.Hub.Infrastructure.Services;
using Musicratic.Shared.Application;
using Musicratic.Shared.Contracts;

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
        services.AddScoped<IMemberListAssignmentRepository, MemberListAssignmentRepository>();
        services.AddScoped<IUnitOfWork, HubUnitOfWork>();
        services.AddScoped<IHubCodeGenerator, HubCodeGenerator>();
        services.AddSingleton<IPlayModeService, PlayModeService>();
        services.AddSingleton<IHubLinkService>(sp =>
            new HubLinkService(configuration));
        services.AddSingleton<IQrCodeService, QrCodeService>();
        services.AddScoped<IHubMemberRoleProvider, HubMemberRoleProvider>();

        services.Configure<AttachmentExpiryOptions>(options =>
        {
            var section = configuration.GetSection(AttachmentExpiryOptions.SectionName);
            var intervalStr = section[nameof(AttachmentExpiryOptions.IntervalMinutes)];
            if (int.TryParse(intervalStr, out var interval))
                options.IntervalMinutes = interval;
        });
        services.AddHostedService<AttachmentExpiryBackgroundService>();

        return services;
    }
}
