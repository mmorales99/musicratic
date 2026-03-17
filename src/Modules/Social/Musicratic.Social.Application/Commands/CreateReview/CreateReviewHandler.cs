using Musicratic.Social.Application.DTOs;
using Musicratic.Social.Application.Services;
using Musicratic.Social.Domain.Entities;
using Musicratic.Social.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Social.Application.Commands.CreateReview;

/// <summary>
/// One review per user per hub. See docs/09-social-features.md §3 Review Rules.
/// </summary>
public sealed class CreateReviewHandler(
    IHubReviewRepository reviewRepository,
    IReviewEventPublisher reviewEventPublisher,
    IUnitOfWork unitOfWork) : ICommandHandler<CreateReviewCommand, ReviewDto>
{
    public async Task<ReviewDto> Handle(
        CreateReviewCommand request, CancellationToken cancellationToken)
    {
        var existing = await reviewRepository.GetByUserAndHub(
            request.HubId, request.UserId, cancellationToken);

        if (existing is not null)
            throw new InvalidOperationException("User has already reviewed this hub.");

        var review = HubReview.Create(
            request.HubId, request.UserId, request.Rating, request.Comment);

        await reviewRepository.Add(review, cancellationToken);
        await unitOfWork.SaveChanges(cancellationToken);

        // Publish integration events after successful save
        await reviewEventPublisher.PublishReviewCreated(
            review.Id, review.HubId, review.UserId, review.Rating, cancellationToken);

        var (averageRating, reviewCount) = await reviewRepository.GetHubRating(
            review.HubId, cancellationToken);

        await reviewEventPublisher.PublishHubRatingChanged(
            review.HubId, averageRating, reviewCount, cancellationToken);

        return new ReviewDto(
            review.Id,
            review.HubId,
            review.UserId,
            review.Rating,
            review.Comment,
            review.OwnerResponse,
            review.CreatedAt);
    }
}
