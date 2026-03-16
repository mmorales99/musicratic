using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Musicratic.Playback.Application.DTOs;
using Musicratic.Playback.Application.Services;

namespace Musicratic.Playback.Infrastructure.Services;

public sealed class QueueBroadcastService(
    IHubConnectionManager connectionManager,
    ILogger<QueueBroadcastService> logger) : IQueueBroadcastService
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public async Task BroadcastNowPlaying(
        Guid hubId,
        NowPlayingDto nowPlaying,
        CancellationToken cancellationToken = default)
    {
        var message = CreateEnvelope("NOW_PLAYING", nowPlaying);
        await SendToHub(hubId, message, cancellationToken);
    }

    public async Task BroadcastQueueUpdate(
        Guid hubId,
        IReadOnlyList<QueueEntryDto> entries,
        CancellationToken cancellationToken = default)
    {
        var message = CreateEnvelope("QUEUE_UPDATED", entries);
        await SendToHub(hubId, message, cancellationToken);
    }

    public async Task BroadcastTrackEnded(
        Guid hubId,
        Guid trackId,
        CancellationToken cancellationToken = default)
    {
        var message = CreateEnvelope("TRACK_ENDED", new { TrackId = trackId });
        await SendToHub(hubId, message, cancellationToken);
    }

    public async Task BroadcastTrackSkipped(
        Guid hubId,
        Guid trackId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        var message = CreateEnvelope("TRACK_SKIPPED", new { TrackId = trackId, Reason = reason });
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
        var connectionCount = connectionManager.GetConnectionIds(hubId).Count;

        logger.LogInformation(
            "Broadcasting {MessageType} to {Count} connections in hub {HubId}",
            message[..Math.Min(30, message.Length)], connectionCount, hubId);

        await connectionManager.SendToHub(hubId, message, cancellationToken);
    }
}
