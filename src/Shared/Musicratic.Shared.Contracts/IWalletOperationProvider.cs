using Musicratic.Shared.Contracts.DTOs;

namespace Musicratic.Shared.Contracts;

public interface IWalletOperationProvider
{
    Task<WalletDebitResult> DebitCoins(
        Guid userId,
        Guid tenantId,
        int amount,
        string reason,
        Guid? referenceId,
        CancellationToken ct);
}
