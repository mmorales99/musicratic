using Microsoft.EntityFrameworkCore;
using Musicratic.Notification.Domain.Entities;
using Musicratic.Shared.Infrastructure;
using Musicratic.Shared.Infrastructure.Persistence;

namespace Musicratic.Notification.Infrastructure.Persistence;

public sealed class NotificationDbContext : BaseDbContext
{
    public DbSet<Domain.Entities.Notification> Notifications => Set<Domain.Entities.Notification>();

    public DbSet<DeviceToken> DeviceTokens => Set<DeviceToken>();

    public NotificationDbContext(
        DbContextOptions<NotificationDbContext> options,
        TenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("notification");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(NotificationDbContext).Assembly);
    }
}
