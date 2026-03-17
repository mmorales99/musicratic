using Musicratic.Social.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Social.Application.Queries.GetHubRating;

/// <summary>
/// Minimum 3 reviews before rating is shown publicly.
/// See docs/09-social-features.md §3 Aggregate Rating.
/// </summary>
public sealed class GetHubRatingQueryHandler(
    IHubReviewRepository hubReviewRepository) : IQueryHandler<GetHubRatingQuery, HubRatingDto>
{
    private const int MinReviewsForPublicRating = 3;

    public async Task<HubRatingDto> Handle(GetHubRatingQuery request, CancellationToken cancellationToken)
    {
        var (averageRating, reviewCount) = await hubReviewRepository.GetHubRating(
            request.HubId, cancellationToken);

        var isPublic = reviewCount >= MinReviewsForPublicRating;

        return new HubRatingDto(
            HubId: request.HubId,
            AverageRating: isPublic ? averageRating : 0,
            ReviewCount: reviewCount,
            IsPublic: isPublic);
    }
}
