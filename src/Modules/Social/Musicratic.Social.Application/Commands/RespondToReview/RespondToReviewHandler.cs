using Musicratic.Social.Application.DTOs;
using Musicratic.Social.Domain.Repositories;
using Musicratic.Shared.Application;
using Musicratic.Shared.Contracts;

namespace Musicratic.Social.Application.Commands.RespondToReview;

/// <summary>
/// Hub owners can respond to reviews (one response per review).
/// See docs/09-social-features.md §3 Review Rules.
/// </summary>
public sealed class RespondToReviewHandler(
    IHubReviewRepository reviewRepository,
    IHubMemberRoleProvider hubMemberRoleProvider,
    ISocialUnitOfWork unitOfWork) : ICommandHandler<RespondToReviewCommand, ReviewDto>
{
    private const string SuperOwnerRole = "SuperOwner";

    public async Task<ReviewDto> Handle(
        RespondToReviewCommand request, CancellationToken cancellationToken)
    {
        var review = await reviewRepository.GetById(request.ReviewId, cancellationToken)
            ?? throw new InvalidOperationException($"Review '{request.ReviewId}' not found.");

        var roleInfo = await hubMemberRoleProvider.GetMemberRole(
            review.HubId, request.HubOwnerId, cancellationToken);

        if (roleInfo is null || roleInfo.Role != SuperOwnerRole)
            throw new UnauthorizedAccessException("Only the hub owner can respond to reviews.");

        review.AddOwnerResponse(request.Response);
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
