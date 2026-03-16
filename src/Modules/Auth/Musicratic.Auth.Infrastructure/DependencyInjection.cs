using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Auth.Application.Services;
using Musicratic.Auth.Domain.Repositories;
using Musicratic.Auth.Infrastructure.Authentication;
using Musicratic.Auth.Infrastructure.Configuration;
using Musicratic.Auth.Infrastructure.Persistence;
using Musicratic.Auth.Infrastructure.Services;
using Musicratic.Shared.Application;

namespace Musicratic.Auth.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddAuthInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AuthDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("AuthDb")));

        services.Configure<AuthentikOptions>(
            configuration.GetSection(AuthentikOptions.SectionName));

        services.AddHttpClient("Authentik");

        services.AddSingleton<IOidcDiscoveryService, OidcDiscoveryService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, AuthUnitOfWork>();

        services.AddAuthentikAuthentication(configuration);

        return services;
    }
}
