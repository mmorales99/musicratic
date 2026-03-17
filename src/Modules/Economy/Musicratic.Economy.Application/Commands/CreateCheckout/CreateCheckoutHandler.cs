using Musicratic.Economy.Application.Services;
using Musicratic.Shared.Application;

namespace Musicratic.Economy.Application.Commands.CreateCheckout;

/// <summary>
/// ECON-008: Handles creating a Stripe checkout session.
/// </summary>
public sealed class CreateCheckoutHandler(
    IStripeService stripeService)
    : ICommandHandler<CreateCheckoutCommand, CreateCheckoutResult>
{
    public async Task<CreateCheckoutResult> Handle(
        CreateCheckoutCommand request,
        CancellationToken cancellationToken)
    {
        var result = await stripeService.CreateCheckoutSession(
            request.UserId, request.CoinPackageId, request.TenantId, cancellationToken);

        return new CreateCheckoutResult(
            result.SessionId, result.Url, result.Success, result.ErrorMessage);
    }
}
