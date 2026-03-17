namespace Musicratic.Economy.Application.Services;

/// <summary>
/// ECON-007: Stripe webhook handler abstraction (application layer).
/// Infrastructure layer provides the implementation.
/// </summary>
public interface IStripeWebhookHandler
{
    Task<WebhookResult> HandleCheckoutCompleted(
        string payload,
        string signature,
        CancellationToken cancellationToken = default);
}

public sealed record WebhookResult(
    bool Success,
    string? ErrorMessage = null);
