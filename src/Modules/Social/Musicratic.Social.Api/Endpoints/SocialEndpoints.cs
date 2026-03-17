using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Social.Application.Commands.CreateReview;
using Musicratic.Social.Application.Commands.DeleteReview;
using Musicratic.Social.Application.Commands.RespondToReview;
using Musicratic.Social.Application.Commands.UpdateReview;
using Musicratic.Social.Application.DTOs;
using Musicratic.Social.Application.Queries.GetHubRating;
using Musicratic.Social.Application.Queries.GetUserProfile;
using Musicratic.Social.Application.Queries.SearchHubs;
using Musicratic.Social.Application.Services;
using Musicratic.Social.Domain.Repositories;

namespace Musicratic.Social.Api.Endpoints;

public static class SocialEndpoints
{
    public static RouteGroupBuilder MapSocialEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/social").WithTags("Social");

        // Reviews
        group.MapPost("/hubs/{hubId:guid}/reviews", CreateReview)
            .WithName("CreateReview")
            .RequireAuthorization();

        group.MapPut("/reviews/{reviewId:guid}", UpdateReview)
            .WithName("UpdateReview")
            .RequireAuthorization();

        group.MapDelete("/reviews/{reviewId:guid}", DeleteReview)
            .WithName("DeleteReview")
            .RequireAuthorization();

        group.MapPost("/reviews/{reviewId:guid}/response", RespondToReview)
            .WithName("RespondToReview")
            .RequireAuthorization();

        group.MapGet("/hubs/{hubId:guid}/reviews", GetHubReviews)
            .WithName("GetHubReviews");

        group.MapGet("/hubs/{hubId:guid}/rating", GetHubRating)
            .WithName("GetHubRating");

        // Discovery
        group.MapGet("/hubs/discover", DiscoverHubs)
            .WithName("DiscoverHubs");

        // Profiles
        group.MapGet("/users/{userId:guid}/profile", GetUserProfile)
            .WithName("GetUserProfile");

        // Sharing
        group.MapGet("/share/hub/{hubId:guid}", GetHubShareLink)
            .WithName("GetHubShareLink");

        return group;
    }

    private static async Task<IResult> CreateReview(
        Guid hubId,
        CreateReviewRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateReviewCommand(
            hubId, request.UserId, request.Rating, request.Comment);

        var result = await sender.Send(command, cancellationToken);
        return Results.Created($"/api/social/reviews/{result.Id}", result);
    }

    private static async Task<IResult> UpdateReview(
        Guid reviewId,
        UpdateReviewRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new UpdateReviewCommand(
            reviewId, request.UserId, request.Rating, request.Comment);

        var result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> DeleteReview(
        Guid reviewId,
        Guid userId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeleteReviewCommand(reviewId, userId), cancellationToken);
        return Results.NoContent();
    }

    private static async Task<IResult> RespondToReview(
        Guid reviewId,
        RespondToReviewRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new RespondToReviewCommand(
            reviewId, request.HubOwnerId, request.Response);

        var result = await sender.Send(command, cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> GetHubReviews(
        Guid hubId,
        IHubReviewRepository reviewRepository,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var reviews = await reviewRepository.GetByHub(hubId, page, pageSize, cancellationToken);
        var totalCount = await reviewRepository.GetCountByHub(hubId, cancellationToken);

        var items = reviews.Select(r => new ReviewDto(
            r.Id, r.HubId, r.UserId, r.Rating,
            r.Comment, r.OwnerResponse, r.CreatedAt)).ToList();

        return Results.Ok(new
        {
            success = true,
            total_items_in_response = items.Count,
            has_more_items = page * pageSize < totalCount,
            items,
            audit = new { retrieved_at = DateTime.UtcNow }
        });
    }

    private static async Task<IResult> GetHubRating(
        Guid hubId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new GetHubRatingQuery(hubId), cancellationToken);
        return Results.Ok(result);
    }

    private static async Task<IResult> DiscoverHubs(
        ISender sender,
        string? search = null,
        string? genres = null,
        string? sortBy = null,
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        pageSize = Math.Clamp(pageSize, 1, 100);
        page = Math.Max(page, 1);

        var query = new SearchHubsQuery(
            SearchTerm: search,
            IsActive: true,
            SortBy: sortBy,
            Page: page,
            PageSize: pageSize);

        var result = await sender.Send(query, cancellationToken);

        return Results.Ok(new
        {
            success = true,
            total_items_in_response = result.Items.Count,
            has_more_items = page * pageSize < result.TotalCount,
            items = result.Items,
            audit = new { retrieved_at = DateTime.UtcNow }
        });
    }

    private static async Task<IResult> GetUserProfile(
        Guid userId,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var profile = await sender.Send(new GetUserProfileQuery(userId), cancellationToken);

        if (profile is null)
        {
            return Results.Problem(
                title: "User not found",
                detail: $"No public profile found for user {userId}.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Results.Ok(profile);
    }

    private static IResult GetHubShareLink(
        Guid hubId,
        ISocialSharingService sharingService,
        string? name = null)
    {
        var hubName = name ?? "Hub";
        var shareLink = sharingService.GenerateHubShareLink(hubId, hubName, description: null);
        return Results.Ok(shareLink);
    }
}

public sealed record CreateReviewRequest(Guid UserId, int Rating, string? Comment);

public sealed record UpdateReviewRequest(Guid UserId, int Rating, string? Comment);

public sealed record RespondToReviewRequest(Guid HubOwnerId, string Response);
