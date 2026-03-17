using Musicratic.Social.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Social.Application.Commands.UpdateReview;

public sealed record UpdateReviewCommand(
    Guid ReviewId,
    Guid UserId,
    int Rating,
    string? Comment) : ICommand<ReviewDto>;
