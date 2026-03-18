using Musicratic.Voting.Application;

namespace Musicratic.Voting.Infrastructure.Persistence;

public sealed class VotingUnitOfWork : IVotingUnitOfWork
{
    private readonly VotingDbContext _dbContext;

    public VotingUnitOfWork(VotingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<int> SaveChanges(CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}
