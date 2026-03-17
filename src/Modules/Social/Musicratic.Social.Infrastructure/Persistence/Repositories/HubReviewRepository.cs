using Microsoft.EntityFrameworkCore;
using Musicratic.Social.Domain.Entities;
using Musicratic.Social.Domain.Repositories;

namespace Musicratic.Social.Infrastructure.Persistence.Repositories;

public sealed class HubReviewRepository : IHubReviewRepository
{
    private readonly SocialDbContext _dbContext;

    public HubReviewRepository(SocialDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<HubReview?> GetById(
        Guid reviewId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HubReviews
            .FirstOrDefaultAsync(r => r.Id == reviewId, cancellationToken);
    }

    public async Task<HubReview?> GetByUserAndHub(
        Guid hubId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HubReviews
            .FirstOrDefaultAsync(
                r => r.HubId == hubId && r.UserId == userId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<HubReview>> GetByHub(
        Guid hubId,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HubReviews
            .Where(r => r.HubId == hubId)
            .OrderByDescending(r => r.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);
    }

    public async Task<(double AverageRating, int ReviewCount)> GetHubRating(
        Guid hubId,
        CancellationToken cancellationToken = default)
    {
        var reviews = _dbContext.HubReviews
            .Where(r => r.HubId == hubId);

        var count = await reviews.CountAsync(cancellationToken);

        if (count == 0)
            return (0, 0);

        var average = await reviews.AverageAsync(r => r.Rating, cancellationToken);

        return (average, count);
    }

    public async Task<int> GetCountByHub(
        Guid hubId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.HubReviews
            .Where(r => r.HubId == hubId)
            .CountAsync(cancellationToken);
    }

    public async Task Add(HubReview review, CancellationToken cancellationToken = default)
    {
        await _dbContext.HubReviews.AddAsync(review, cancellationToken);
    }

    public void Remove(HubReview review)
    {
        _dbContext.HubReviews.Remove(review);
    }
}
