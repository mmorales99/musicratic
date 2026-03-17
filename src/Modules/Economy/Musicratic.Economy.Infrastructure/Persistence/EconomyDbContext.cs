using Microsoft.EntityFrameworkCore;
using Musicratic.Economy.Domain.Entities;
using Musicratic.Shared.Infrastructure;
using Musicratic.Shared.Infrastructure.Persistence;

namespace Musicratic.Economy.Infrastructure.Persistence;

public sealed class EconomyDbContext : BaseDbContext
{
    public DbSet<Wallet> Wallets => Set<Wallet>();

    public DbSet<Transaction> Transactions => Set<Transaction>();

    public DbSet<CoinPackage> CoinPackages => Set<CoinPackage>();

    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public EconomyDbContext(
        DbContextOptions<EconomyDbContext> options,
        TenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("economy");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(EconomyDbContext).Assembly);
    }
}
