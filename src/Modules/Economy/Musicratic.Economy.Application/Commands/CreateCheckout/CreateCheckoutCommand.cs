using Musicratic.Shared.Application;

namespace Musicratic.Economy.Application.Commands.CreateCheckout;

/// <summary>
/// ECON-008: Creates a Stripe checkout session for coin purchase.
/// </summary>
public sealed record CreateCheckoutCommand(
    Guid UserId,
    Guid CoinPackageId,
    Guid TenantId) : ICommand<CreateCheckoutResult>;

public sealed record CreateCheckoutResult(
    string SessionId,
    string Url,
    bool Success,
    string? ErrorMessage = null);
