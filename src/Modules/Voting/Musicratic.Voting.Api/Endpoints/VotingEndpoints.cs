using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Voting.Application.Commands.CastVote;
using Musicratic.Voting.Application.Commands.OpenCollectiveVote;
using Musicratic.Voting.Application.Commands.RemoveVote;
using Musicratic.Voting.Application.Queries.GetCollectiveVoteSession;
using Musicratic.Voting.Application.Queries.GetTally;
using Musicratic.Voting.Domain.Enums;

namespace Musicratic.Voting.Api.Endpoints;

/// <summary>
/// VOTE-012: Voting API endpoints with Problem Details (RFC 9457) errors.
/// </summary>
public static class VotingEndpoints
{
    public static RouteGroupBuilder MapVotingEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/hubs/{hubId:guid}").WithTags("Voting");

        group.MapPost("/queue/{entryId:guid}/vote", CastVote).WithName("CastVote");
        group.MapGet("/queue/{entryId:guid}/tally", GetTally).WithName("GetTally");
        group.MapDelete("/queue/{entryId:guid}/vote", RemoveVote).WithName("RemoveVote");
        group.MapPost("/collective-vote", OpenCollectiveVote).WithName("OpenCollectiveVote");
        group.MapGet("/collective-vote/{sessionId:guid}", GetCollectiveVoteSession)
            .WithName("GetCollectiveVoteSession");

        return group;
    }

    private static async Task<IResult> CastVote(
        Guid hubId,
        Guid entryId,
        CastVoteRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId(httpContext);
        if (userId is null)
            return Results.Unauthorized();

        if (!Enum.TryParse<VoteValue>(request.Value, true, out var voteValue))
        {
            return Results.Problem(
                title: "Invalid vote value",
                detail: "Vote value must be 'Up' or 'Down'.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        try
        {
            var result = await sender.Send(
                new CastVoteCommand(hubId, userId.Value, entryId, voteValue),
                cancellationToken);

            return Results.Created($"/api/hubs/{hubId}/queue/{entryId}/vote", result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                title: "Vote failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
    }

    private static async Task<IResult> GetTally(
        Guid hubId,
        Guid entryId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetTallyQuery(entryId), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> RemoveVote(
        Guid hubId,
        Guid entryId,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId(httpContext);
        if (userId is null)
            return Results.Unauthorized();

        try
        {
            await sender.Send(
                new RemoveVoteCommand(hubId, userId.Value, entryId),
                cancellationToken);

            return Results.NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                title: "Remove vote failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status404NotFound);
        }
    }

    private static async Task<IResult> OpenCollectiveVote(
        Guid hubId,
        OpenCollectiveVoteRequest request,
        ISender sender,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var userId = ExtractUserId(httpContext);
        if (userId is null)
            return Results.Unauthorized();

        try
        {
            var result = await sender.Send(
                new OpenCollectiveVoteCommand(hubId, request.QueueEntryId, userId.Value),
                cancellationToken);

            return Results.Created(
                $"/api/hubs/{hubId}/collective-vote/{result.Id}", result);
        }
        catch (InvalidOperationException ex)
        {
            return Results.Problem(
                title: "Collective vote failed",
                detail: ex.Message,
                statusCode: StatusCodes.Status409Conflict);
        }
    }

    private static async Task<IResult> GetCollectiveVoteSession(
        Guid hubId,
        Guid sessionId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(
            new GetCollectiveVoteSessionQuery(sessionId), cancellationToken);

        return result is null ? Results.NotFound() : Results.Ok(result);
    }

    private static Guid? ExtractUserId(HttpContext context)
    {
        var sub = context.User.FindFirst("sub")?.Value
                  ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        return Guid.TryParse(sub, out var userId) ? userId : null;
    }

    public sealed record CastVoteRequest(string Value);

    public sealed record OpenCollectiveVoteRequest(Guid QueueEntryId);
}
