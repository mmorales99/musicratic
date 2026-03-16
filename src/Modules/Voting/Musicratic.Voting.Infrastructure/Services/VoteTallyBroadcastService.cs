using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Musicratic.Voting.Application.DTOs;
using Musicratic.Voting.Application.Services;

namespace Musicratic.Voting.Infrastructure.Services;

/// <summary>
/// VOTE-009: Broadcasts vote tally updates to all hub members via WebSocket.
/// Message types: VOTE_CAST, TALLY_UPDATED, SKIP_TRIGGERED.
/// Typed message envelopes per docs/10-platform-and-tech-stack.md.
/// </summary>
public sealed class VoteTallyBroadcastService(
    IVoteConnectionManager connectionManager,
    ILogger<VoteTallyBroadcastService> logger) : IVoteTallyBroadcastService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task BroadcastVoteCast(
        Guid hubId, VoteTallyDto tally, CancellationToken cancellationToken = default)
    {
        var message = CreateEnvelope("VOTE_CAST", tally);
        await SendToHub(hubId, message, cancellationToken);
    }

    public async Task BroadcastTallyUpdated(
        Guid hubId, VoteTallyDto tally, CancellationToken cancellationToken = default)
    {
        var message = CreateEnvelope("TALLY_UPDATED", tally);
        await SendToHub(hubId, message, cancellationToken);
    }

    public async Task BroadcastSkipTriggered(
        Guid hubId, Guid queueEntryId, string reason, CancellationToken cancellationToken = default)
    {
        var message = CreateEnvelope("SKIP_TRIGGERED",
            new { QueueEntryId = queueEntryId, Reason = reason });
        await SendToHub(hubId, message, cancellationToken);
    }

    private static string CreateEnvelope(string type, object payload)
    {
        var envelope = new { Type = type, Payload = payload };
        return JsonSerializer.Serialize(envelope, JsonOptions);
    }

    private async Task SendToHub(
        Guid hubId, string message, CancellationToken cancellationToken)
    {
        var count = connectionManager.GetConnectionCount(hubId);

        logger.LogInformation(
            "Broadcasting vote event to {Count} connections in hub {HubId}",
            count, hubId);

        await connectionManager.SendToHub(hubId, message, cancellationToken);
    }
}
