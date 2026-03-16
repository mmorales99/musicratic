using Musicratic.Shared.Application;

namespace Musicratic.Auth.Application.Commands.CreditWallet;

public sealed record CreditWalletCommand(
    Guid UserId,
    int Amount) : ICommand;
