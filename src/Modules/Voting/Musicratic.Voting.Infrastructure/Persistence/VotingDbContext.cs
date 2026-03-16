using Microsoft.EntityFrameworkCore;
using Musicratic.Shared.Infrastructure;
using Musicratic.Shared.Infrastructure.Persistence;
using Musicratic.Voting.Domain.Entities;

namespace Musicratic.Voting.Infrastructure.Persistence;

public sealed class VotingDbContext : BaseDbContext
{
    public DbSet<Vote> Votes => Set<Vote>();

    public DbSet<CollectiveVoteSession> CollectiveVoteSessions => Set<CollectiveVoteSession>();

    public VotingDbContext(
        DbContextOptions<VotingDbContext> options,
        TenantContext tenantContext)
        : base(options, tenantContext)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema("voting");
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(VotingDbContext).Assembly);
    }
}
