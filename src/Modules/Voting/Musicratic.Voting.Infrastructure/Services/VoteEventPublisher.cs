using Dapr.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Musicratic.Voting.Application.Services;
using Musicratic.Voting.Domain.Enums;

namespace Musicratic.Voting.Infrastructure.Services;

/// <summary>
/// VOTE-013: Publishes voting integration events via Dapr pub/sub.
/// Topic format: {env}_{feature}_{action} per docs/10-platform-and-tech-stack.md.
/// Tenant ID in message metadata, not topic name.
/// </summary>
public sealed class VoteEventPublisher(
    DaprClient daprClient,
    IConfiguration configuration,
    ILogger<VoteEventPublisher> logger) : IVoteEventPublisher
{
    private const string PubSubName = "pubsub";

    private string Env => configuration["MUSICRATIC_ENVIRONMENT"] ?? "dev";

    public async Task PublishVoteCastAsync(
        Guid tenantId, Guid queueEntryId, Guid userId, VoteValue value,
        CancellationToken cancellationToken = default)
    {
        var topicName = $"{Env}_voting_vote-cast";
        var eventData = new VoteCastEventData(tenantId, queueEntryId, userId, value.ToString());
        var metadata = new Dictionary<string, string> { ["tenantId"] = tenantId.ToString() };

        await PublishAsync(topicName, eventData, metadata, cancellationToken);
    }

    public async Task PublishSkipTriggeredAsync(
        Guid tenantId, Guid queueEntryId, string reason, double downvotePercentage,
        CancellationToken cancellationToken = default)
    {
        var topicName = $"{Env}_voting_skip-triggered";
        var eventData = new SkipTriggeredEventData(tenantId, queueEntryId, reason, downvotePercentage);
        var metadata = new Dictionary<string, string> { ["tenantId"] = tenantId.ToString() };

        await PublishAsync(topicName, eventData, metadata, cancellationToken);
    }

    private async Task PublishAsync<T>(
        string topicName, T eventData, Dictionary<string, string> metadata,
        CancellationToken cancellationToken) where T : class
    {
        try
        {
            logger.LogInformation("Publishing {Topic}", topicName);
            await daprClient.PublishEventAsync(
                PubSubName, topicName, eventData, metadata, cancellationToken);
        }
        catch (Exception ex)
        {
            // Dapr sidecar may not be running in dev. Log and continue.
            logger.LogWarning(ex, "Failed to publish Dapr event {Topic}", topicName);
        }
    }

    internal sealed record VoteCastEventData(
        Guid TenantId, Guid QueueEntryId, Guid UserId, string Value);

    internal sealed record SkipTriggeredEventData(
        Guid TenantId, Guid QueueEntryId, string Reason, double DownvotePercentage);
}
