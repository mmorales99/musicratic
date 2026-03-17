using Musicratic.Social.Application.DTOs;
using Musicratic.Social.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Social.Application.Commands.UpdateReview;

public sealed class UpdateReviewHandler(
    IHubReviewRepository reviewRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<UpdateReviewCommand, ReviewDto>
{
    public async Task<ReviewDto> Handle(
        UpdateReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetById(request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException($"Review '{request.ReviewId}' not found.");

        if (review.UserId != request.UserId)
            throw new UnauthorizedAccessException("Only the review author can update this review.");

        review.Update(request.Rating, request.Comment);
        await unitOfWork.SaveChanges(cancellationToken);

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
