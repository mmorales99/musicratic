using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Auth.Domain.Repositories;
using Musicratic.Auth.Infrastructure.Persistence;
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

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUnitOfWork, AuthUnitOfWork>();

        return services;
    }
}
