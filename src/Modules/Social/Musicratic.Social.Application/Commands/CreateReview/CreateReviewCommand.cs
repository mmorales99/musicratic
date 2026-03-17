using Musicratic.Social.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Social.Application.Commands.CreateReview;

public sealed record CreateReviewCommand(
    Guid HubId,
    Guid UserId,
    int Rating,
    string? Comment) : ICommand<ReviewDto>;
