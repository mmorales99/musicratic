namespace Musicratic.Economy.Application.Services;

/// <summary>
/// ECON-009: Apple In-App Purchase receipt verification.
/// </summary>
public interface IAppleIapService
{
    Task<IapVerificationResult> VerifyReceipt(
        string receiptData,
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default);
}

public sealed record IapVerificationResult(
    bool Success,
    int CoinsAwarded,
    string? ErrorMessage = null);
