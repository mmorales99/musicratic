using Musicratic.Social.Domain.Events;
using Musicratic.Shared.Domain;

namespace Musicratic.Social.Domain.Entities;

public sealed class HubReview : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; private set; }

    public Guid HubId { get; private set; }

    public Guid UserId { get; private set; }

    /// <summary>Rating from 1 to 5. See docs/09-social-features.md.</summary>
    public int Rating { get; private set; }

    public string? Comment { get; private set; }

    public string? OwnerResponse { get; private set; }

    private HubReview() { }

    public static HubReview Create(Guid hubId, Guid userId, int rating, string? comment)
    {
        ValidateRating(rating);
        ValidateComment(comment);

        var review = new HubReview
        {
            TenantId = hubId,
            HubId = hubId,
            UserId = userId,
            Rating = rating,
            Comment = comment?.Trim()
        };

        review.AddDomainEvent(new ReviewCreatedEvent(hubId, userId, rating));

        return review;
    }

    public void Update(int rating, string? comment)
    {
        ValidateRating(rating);
        ValidateComment(comment);

        Rating = rating;
        Comment = comment?.Trim();

        AddDomainEvent(new ReviewUpdatedEvent(HubId, Id));
    }

    public void AddOwnerResponse(string response)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(response);

        if (response.Length > 500)
            throw new ArgumentException("Owner response cannot exceed 500 characters.");

        OwnerResponse = response.Trim();
    }

    private static void ValidateRating(int rating)
    {
        if (rating is < 1 or > 5)
            throw new ArgumentOutOfRangeException(nameof(rating), "Rating must be between 1 and 5.");
    }

    private static void ValidateComment(string? comment)
    {
        if (comment is { Length: > 500 })
            throw new ArgumentException("Comment cannot exceed 500 characters.");
    }
}
