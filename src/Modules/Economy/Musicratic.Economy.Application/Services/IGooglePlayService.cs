namespace Musicratic.Economy.Application.Services;

/// <summary>
/// ECON-010: Google Play billing verification.
/// </summary>
public interface IGooglePlayService
{
    Task<PurchaseVerificationResult> VerifyPurchase(
        string purchaseToken,
        string productId,
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}

public sealed record PurchaseVerificationResult(
    bool Success,
    int CoinsAwarded,
    string? ErrorMessage = null);
