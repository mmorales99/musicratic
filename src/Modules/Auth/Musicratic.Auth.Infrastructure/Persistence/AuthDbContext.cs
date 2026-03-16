using Microsoft.EntityFrameworkCore;
using Musicratic.Auth.Domain.Entities;
using Musicratic.Shared.Infrastructure;
using Musicratic.Shared.Infrastructure.Persistence;

namespace Musicratic.Auth.Infrastructure.Persistence;

public sealed class AuthDbContext : BaseDbContext
{
    public DbSet<User> Users => Set<User>();

    public AuthDbContext(
        DbContextOptions<AuthDbContext> options,
        TenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("auth");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthDbContext).Assembly);
    }
}
