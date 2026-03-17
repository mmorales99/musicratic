using Musicratic.Shared.Application;

namespace Musicratic.Social.Application.Commands.DeleteReview;

public sealed record DeleteReviewCommand(
    Guid ReviewId,
    Guid UserId) : ICommand;
