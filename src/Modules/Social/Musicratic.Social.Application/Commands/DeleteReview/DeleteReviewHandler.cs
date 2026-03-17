using Musicratic.Social.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Social.Application.Commands.DeleteReview;

public sealed class DeleteReviewHandler(
    IHubReviewRepository reviewRepository,
    IUnitOfWork unitOfWork) : ICommandHandler<DeleteReviewCommand>
{
    public async Task Handle(
        DeleteReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetById(request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException($"Review '{request.ReviewId}' not found.");

        if (review.UserId != request.UserId)
            throw new UnauthorizedAccessException("Only the review author can delete this review.");

        reviewRepository.Remove(review);
        await unitOfWork.SaveChanges(cancellationToken);
    }
}
