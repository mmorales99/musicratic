using Microsoft.EntityFrameworkCore;
using Musicratic.Social.Domain.Entities;
using Musicratic.Shared.Infrastructure;
using Musicratic.Shared.Infrastructure.Persistence;

namespace Musicratic.Social.Infrastructure.Persistence;

public sealed class SocialDbContext : BaseDbContext
{
    public DbSet<HubReview> HubReviews => Set<HubReview>();

    public SocialDbContext(
        DbContextOptions<SocialDbContext> options,
        TenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("social");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SocialDbContext).Assembly);
    }
}
