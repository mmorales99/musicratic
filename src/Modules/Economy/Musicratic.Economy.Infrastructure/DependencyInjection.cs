using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Economy.Application.Services;
using Musicratic.Economy.Domain.Repositories;
using Musicratic.Economy.Infrastructure.Persistence;
using Musicratic.Economy.Infrastructure.Services;
using Musicratic.Shared.Application;

namespace Musicratic.Economy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEconomyInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<EconomyDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("EconomyDb")));

        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ICoinPackageRepository, CoinPackageRepository>();
        services.AddScoped<IUnitOfWork, EconomyUnitOfWork>();
        services.AddScoped<IRefundService, RefundService>();

        return services;
    }
}
