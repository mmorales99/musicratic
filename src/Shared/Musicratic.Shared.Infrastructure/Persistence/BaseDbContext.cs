using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Musicratic.Shared.Domain;

namespace Musicratic.Shared.Infrastructure.Persistence;

public abstract class BaseDbContext(
    DbContextOptions options,
    TenantContext tenantContext) : DbContext(options)
{
    protected TenantContext TenantContext { get; } = tenantContext;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        ApplyTenantQueryFilters(modelBuilder);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SetAuditableFields();
        return base.SaveChangesAsync(cancellationToken);
    }

    private void SetAuditableFields()
    {
        var utcNow = DateTime.UtcNow;

        foreach (var entry in ChangeTracker.Entries<BaseEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedAt = utcNow;
                    entry.Entity.UpdatedAt = utcNow;
                    break;
                case EntityState.Modified:
                    entry.Entity.UpdatedAt = utcNow;
                    break;
            }
        }
    }

    private static void ApplyTenantQueryFilters(ModelBuilder modelBuilder)
    {
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(ITenantScoped).IsAssignableFrom(entityType.ClrType))
            {
                continue;
            }

            modelBuilder.Entity(entityType.ClrType)
                .HasQueryFilter(CreateTenantFilter(entityType.ClrType));
        }
    }

    private static LambdaExpression CreateTenantFilter(Type entityType)
    {
        var parameter = System.Linq.Expressions.Expression.Parameter(entityType, "e");
        // Filter will be applied dynamically at query time via interceptor
        // This is a placeholder that always returns true; actual filtering
        // is done via the TenantContext in the module-specific DbContext
        var body = System.Linq.Expressions.Expression.Constant(true);
        return System.Linq.Expressions.Expression.Lambda(body, parameter);
    }
}
