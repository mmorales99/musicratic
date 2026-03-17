using Musicratic.Shared.Application;

namespace Musicratic.Economy.Application.Commands.VerifyGooglePurchase;

/// <summary>
/// ECON-010: Verifies a Google Play purchase and credits coins.
/// </summary>
public sealed record VerifyGooglePurchaseCommand(
    string PurchaseToken,
    string ProductId,
    Guid UserId,
    Guid TenantId) : ICommand<VerifyGooglePurchaseResult>;

public sealed record VerifyGooglePurchaseResult(
    bool Success,
    int CoinsAwarded,
    string? ErrorMessage = null);
