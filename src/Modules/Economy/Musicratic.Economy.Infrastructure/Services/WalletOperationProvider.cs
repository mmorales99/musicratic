using Microsoft.Extensions.Logging;
using Musicratic.Economy.Domain.Repositories;
using Musicratic.Shared.Application;
using Musicratic.Shared.Contracts;
using Musicratic.Shared.Contracts.DTOs;

namespace Musicratic.Economy.Infrastructure.Services;

public sealed class WalletOperationProvider(
    IWalletRepository walletRepository,
    IUnitOfWork unitOfWork,
    ILogger<WalletOperationProvider> logger) : IWalletOperationProvider
{
    public async Task<WalletDebitResult> DebitCoins(
        Guid userId,
        Guid tenantId,
        int amount,
        string reason,
        Guid? referenceId,
        CancellationToken ct)
    {
        var wallet = await walletRepository.GetByUserAndTenant(userId, tenantId, ct);

        if (wallet is null)
        {
            logger.LogWarning(
                "Wallet not found for user {UserId} in tenant {TenantId}",
                userId, tenantId);

            return new WalletDebitResult(false, 0, "Wallet not found.");
        }

        if (wallet.Balance < amount)
        {
            logger.LogInformation(
                "Insufficient balance for user {UserId}: has {Balance}, needs {Amount}",
                userId, wallet.Balance, amount);

            return new WalletDebitResult(
                false,
                wallet.Balance,
                $"Insufficient balance. Current: {wallet.Balance}, Requested: {amount}.");
        }

        wallet.Debit(amount, reason, referenceId);
        walletRepository.Update(wallet);
        await unitOfWork.SaveChanges(ct);

        logger.LogInformation(
            "Debited {Amount} coins from user {UserId} in tenant {TenantId}. New balance: {Balance}",
            amount, userId, tenantId, wallet.Balance);

        return new WalletDebitResult(true, wallet.Balance, null);
    }
}
