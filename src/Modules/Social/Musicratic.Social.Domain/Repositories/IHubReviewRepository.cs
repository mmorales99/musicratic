using Musicratic.Social.Domain.Entities;

namespace Musicratic.Social.Domain.Repositories;

public interface IHubReviewRepository
{
    Task<HubReview?> GetById(Guid reviewId, CancellationToken cancellationToken = default);

    Task<HubReview?> GetByUserAndHub(Guid hubId, Guid userId, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<HubReview>> GetByHub(Guid hubId, int page, int pageSize, CancellationToken cancellationToken = default);

    Task<int> GetCountByHub(Guid hubId, CancellationToken cancellationToken = default);

    Task<(double AverageRating, int ReviewCount)> GetHubRating(Guid hubId, CancellationToken cancellationToken = default);

    Task Add(HubReview review, CancellationToken cancellationToken = default);

    void Remove(HubReview review);
}
