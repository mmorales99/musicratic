using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Economy.Application.Commands.ProcessSkipRefund;

namespace Musicratic.Economy.Api.Endpoints;

/// <summary>
/// ECON-004: Dapr subscriber endpoint for skip-triggered events.
/// Listens to {env}_voting_skip-triggered topic.
/// </summary>
public static class EconomyDaprSubscriberEndpoints
{
    public static RouteGroupBuilder MapEconomyDaprEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/economy/dapr").WithTags("Economy Dapr");

        group.MapPost("/skip-triggered", HandleSkipTriggered)
            .WithName("EconomySkipTriggered");

        return group;
    }

    /// <summary>
    /// Handles the skip-triggered integration event from Voting module.
    /// Dapr delivers this via pub/sub when a track is skipped.
    /// </summary>
    private static async Task<IResult> HandleSkipTriggered(
        SkipTriggeredRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new ProcessSkipRefundCommand(
            request.TenantId,
            request.QueueEntryId,
            request.Reason,
            request.PlayedDuration,
            request.TotalDuration,
            request.ProposerUserId,
            request.CoinsSpent);

        var result = await sender.Send(command, cancellationToken);

        return Results.Ok(result);
    }
}

/// <summary>
/// DTO for the Dapr skip-triggered event delivery.
/// Enriched with proposer/cost data by the publishing module.
/// </summary>
public sealed record SkipTriggeredRequest(
    Guid TenantId,
    Guid QueueEntryId,
    string Reason,
    TimeSpan PlayedDuration,
    TimeSpan TotalDuration,
    Guid ProposerUserId,
    decimal CoinsSpent);
