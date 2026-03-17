namespace Musicratic.Social.Application.Services;

public interface IReviewEventPublisher
{
    Task PublishReviewCreated(
        Guid reviewId,
        Guid hubId,
        Guid reviewerId,
        int rating,
        CancellationToken cancellationToken = default);

    Task PublishHubRatingChanged(
        Guid hubId,
        double newAverageRating,
        int reviewCount,
        CancellationToken cancellationToken = default);
}
