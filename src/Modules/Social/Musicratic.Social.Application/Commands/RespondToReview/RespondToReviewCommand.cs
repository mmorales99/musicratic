using Musicratic.Social.Application.DTOs;
using Musicratic.Shared.Application;

namespace Musicratic.Social.Application.Commands.RespondToReview;

public sealed record RespondToReviewCommand(
    Guid ReviewId,
    Guid HubOwnerId,
    string Response) : ICommand<ReviewDto>;
