using Microsoft.Extensions.Configuration;
using Musicratic.Shared.Contracts.Events;
using Musicratic.Shared.Infrastructure;
using Musicratic.Social.Application.Services;

namespace Musicratic.Social.Infrastructure.Services;

public sealed class ReviewEventPublisher(
    IDaprEventPublisher daprEventPublisher,
    IConfiguration configuration) : IReviewEventPublisher
{
    private const string DefaultEnvironment = "dev";

    private string Environment =>
        configuration["Musicratic:Environment"] ?? DefaultEnvironment;

    public async Task PublishReviewCreated(
        Guid reviewId,
        Guid hubId,
        Guid reviewerId,
        int rating,
        CancellationToken cancellationToken = default)
    {
        var @event = new ReviewCreatedIntegrationEvent(
            ReviewId: reviewId,
            HubId: hubId,
            ReviewerId: reviewerId,
            ReviewerName: string.Empty,
            TrackId: Guid.Empty,
            TrackTitle: string.Empty,
            Rating: rating);

        var topic = $"{Environment}_social_review-created";

        var metadata = new Dictionary<string, string>
        {
            ["tenantId"] = hubId.ToString()
        };

        await daprEventPublisher.Publish(@event, topic, metadata, cancellationToken);
    }

    public async Task PublishHubRatingChanged(
        Guid hubId,
        double newAverageRating,
        int reviewCount,
        CancellationToken cancellationToken = default)
    {
        var @event = new HubRatedIntegrationEvent(
            HubId: hubId,
            NewAverageRating: newAverageRating,
            ReviewCount: reviewCount);

        var topic = $"{Environment}_social_hub-rated";

        var metadata = new Dictionary<string, string>
        {
            ["tenantId"] = hubId.ToString()
        };

        await daprEventPublisher.Publish(@event, topic, metadata, cancellationToken);
    }
}
