namespace Musicratic.Economy.Application.Services;

/// <summary>
/// ECON-008: Stripe checkout/payment service abstraction.
/// </summary>
public interface IStripeService
{
    Task<CheckoutSessionResult> CreateCheckoutSession(
        Guid userId,
        Guid coinPackageId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}

public sealed record CheckoutSessionResult(
    string SessionId,
    string Url,
    bool Success,
    string? ErrorMessage = null);
