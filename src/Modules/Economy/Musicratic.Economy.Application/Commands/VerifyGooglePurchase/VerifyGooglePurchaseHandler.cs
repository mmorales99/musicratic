using Musicratic.Economy.Application.Services;
using Musicratic.Shared.Application;

namespace Musicratic.Economy.Application.Commands.VerifyGooglePurchase;

/// <summary>
/// ECON-010: Handles Google Play purchase verification.
/// </summary>
public sealed class VerifyGooglePurchaseHandler(
    IGooglePlayService googlePlayService)
    : ICommandHandler<VerifyGooglePurchaseCommand, VerifyGooglePurchaseResult>
{
    public async Task<VerifyGooglePurchaseResult> Handle(
        VerifyGooglePurchaseCommand request,
        CancellationToken cancellationToken)
    {
        var result = await googlePlayService.VerifyPurchase(
            request.PurchaseToken, request.ProductId,
            request.UserId, request.TenantId, cancellationToken);

        return new VerifyGooglePurchaseResult(
            result.Success, result.CoinsAwarded, result.ErrorMessage);
    }
}
