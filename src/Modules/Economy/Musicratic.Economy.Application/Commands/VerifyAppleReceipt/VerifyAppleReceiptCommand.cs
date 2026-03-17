using Musicratic.Shared.Application;

namespace Musicratic.Economy.Application.Commands.VerifyAppleReceipt;

/// <summary>
/// ECON-009: Verifies an Apple IAP receipt and credits coins.
/// </summary>
public sealed record VerifyAppleReceiptCommand(
    string ReceiptData,
    Guid UserId,
    Guid TenantId) : ICommand<VerifyAppleReceiptResult>;

public sealed record VerifyAppleReceiptResult(
    bool Success,
    int CoinsAwarded,
    string? ErrorMessage = null);
