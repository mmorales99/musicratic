using Musicratic.Economy.Application.Services;
using Musicratic.Shared.Application;

namespace Musicratic.Economy.Application.Commands.VerifyAppleReceipt;

/// <summary>
/// ECON-009: Handles Apple IAP receipt verification.
/// </summary>
public sealed class VerifyAppleReceiptHandler(
    IAppleIapService appleIapService)
    : ICommandHandler<VerifyAppleReceiptCommand, VerifyAppleReceiptResult>
{
    public async Task<VerifyAppleReceiptResult> Handle(
        VerifyAppleReceiptCommand request,
        CancellationToken cancellationToken)
    {
        var result = await appleIapService.VerifyReceipt(
            request.ReceiptData, request.UserId, request.TenantId, cancellationToken);

        return new VerifyAppleReceiptResult(
            result.Success, result.CoinsAwarded, result.ErrorMessage);
    }
}
