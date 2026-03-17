using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Musicratic.Economy.Application.Services;
using Musicratic.Economy.Domain.Repositories;
using Musicratic.Economy.Infrastructure.Persistence;
using Musicratic.Economy.Infrastructure.Services;
using Musicratic.Shared.Application;
using Musicratic.Shared.Contracts;

namespace Musicratic.Economy.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddEconomyInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<EconomyDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("EconomyDb")));

        // Repositories
        services.AddScoped<IWalletRepository, WalletRepository>();
        services.AddScoped<ITransactionRepository, TransactionRepository>();
        services.AddScoped<ICoinPackageRepository, CoinPackageRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();
        services.AddScoped<IUnitOfWork, EconomyUnitOfWork>();

        // ECON-004: Refund service
        services.AddScoped<IRefundService, RefundService>();

        // Shared contract: wallet operations for cross-module usage (PLAY-013)
        services.AddScoped<IWalletOperationProvider, WalletOperationProvider>();

        // ECON-005: Coin pricing engine with configurable options
        var pricingSection = configuration.GetSection(CoinPricingOptions.SectionName);
        var pricingOptions = ReadCoinPricingOptions(pricingSection);
        services.AddSingleton(pricingOptions);
        services.AddSingleton<ICoinPricingEngine, CoinPricingEngine>();

        // ECON-007: Stripe webhook handler
        var stripeOptions = ReadStripeOptions(configuration.GetSection(StripeOptions.SectionName));
        services.AddSingleton(Options.Create(stripeOptions));
        services.AddScoped<IStripeWebhookHandler, StripeWebhookHandler>();

        // ECON-008: Stripe checkout service
        services.AddScoped<IStripeService, StripeService>();

        // ECON-009: Apple IAP service
        var appleOptions = ReadAppleIapOptions(configuration.GetSection(AppleIapOptions.SectionName));
        services.AddSingleton(Options.Create(appleOptions));
        services.AddScoped<IAppleIapService, AppleIapService>();

        // ECON-010: Google Play service
        var googleOptions = ReadGooglePlayOptions(configuration.GetSection(GooglePlayOptions.SectionName));
        services.AddSingleton(Options.Create(googleOptions));
        services.AddScoped<IGooglePlayService, GooglePlayService>();

        return services;
    }

    private static CoinPricingOptions ReadCoinPricingOptions(IConfigurationSection section)
    {
        var options = new CoinPricingOptions();
        if (double.TryParse(section["NormalMultiplier"], out var nm)) options.NormalMultiplier = nm;
        if (double.TryParse(section["WarmMultiplier"], out var wm)) options.WarmMultiplier = wm;
        if (double.TryParse(section["HotMultiplier"], out var hm)) options.HotMultiplier = hm;
        if (double.TryParse(section["FireMultiplier"], out var fm)) options.FireMultiplier = fm;
        if (double.TryParse(section["ViralMultiplier"], out var vm)) options.ViralMultiplier = vm;
        if (double.TryParse(section["WarmThreshold"], out var wt)) options.WarmThreshold = wt;
        if (double.TryParse(section["HotThreshold"], out var ht)) options.HotThreshold = ht;
        if (double.TryParse(section["FireThreshold"], out var ft)) options.FireThreshold = ft;
        if (double.TryParse(section["ViralThreshold"], out var vt)) options.ViralThreshold = vt;
        return options;
    }

    private static StripeOptions ReadStripeOptions(IConfigurationSection section)
    {
        return new StripeOptions
        {
            ApiKey = section["ApiKey"],
            WebhookSecret = section["WebhookSecret"],
            SuccessUrl = section["SuccessUrl"] ?? "/payment/success",
            CancelUrl = section["CancelUrl"] ?? "/payment/cancel"
        };
    }

    private static AppleIapOptions ReadAppleIapOptions(IConfigurationSection section)
    {
        var options = new AppleIapOptions { SharedSecret = section["SharedSecret"] };
        if (bool.TryParse(section["UseSandbox"], out var sandbox)) options.UseSandbox = sandbox;
        return options;
    }

    private static GooglePlayOptions ReadGooglePlayOptions(IConfigurationSection section)
    {
        return new GooglePlayOptions
        {
            PackageName = section["PackageName"],
            ServiceAccountToken = section["ServiceAccountToken"]
        };
    }
}
