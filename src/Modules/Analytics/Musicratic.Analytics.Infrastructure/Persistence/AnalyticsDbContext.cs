using Microsoft.EntityFrameworkCore;
using Musicratic.Analytics.Domain.Entities;
using Musicratic.Shared.Infrastructure;
using Musicratic.Shared.Infrastructure.Persistence;

namespace Musicratic.Analytics.Infrastructure.Persistence;

/// <summary>
/// ANLT-003: Analytics module DbContext with schema "analytics".
/// </summary>
public sealed class AnalyticsDbContext : BaseDbContext
{
    public DbSet<TrackStats> TrackStats => Set<TrackStats>();

    public DbSet<HubStats> HubStats => Set<HubStats>();

    public AnalyticsDbContext(
        DbContextOptions<AnalyticsDbContext> options,
        TenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("analytics");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AnalyticsDbContext).Assembly);
    }
}
