using Microsoft.EntityFrameworkCore;
using Musicratic.Hub.Domain.Entities;
using Musicratic.Shared.Infrastructure;
using Musicratic.Shared.Infrastructure.Persistence;

namespace Musicratic.Hub.Infrastructure.Persistence;

public sealed class HubDbContext : BaseDbContext
{
    public DbSet<Domain.Entities.Hub> Hubs => Set<Domain.Entities.Hub>();
    public DbSet<HubMember> HubMembers => Set<HubMember>();
    public DbSet<HubAttachment> HubAttachments => Set<HubAttachment>();
    public DbSet<List> Lists => Set<List>();
    public DbSet<ListTrack> ListTracks => Set<ListTrack>();

    public HubDbContext(
        DbContextOptions<HubDbContext> options,
        TenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("hub");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(HubDbContext).Assembly);
    }
}
