using Musicratic.Economy.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Economy.Domain.Entities;

public sealed class Transaction : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; private set; }

    public Guid WalletId { get; private set; }

    public decimal Amount { get; private set; }

    public TransactionType Type { get; private set; }

    public string Reason { get; private set; } = string.Empty;

    public Guid? ReferenceId { get; private set; }

    private Transaction() { }

    public static Transaction Create(
        Guid walletId,
        decimal amount,
        TransactionType type,
        string reason,
        Guid tenantId,
        Guid? referenceId = null)
    {
        if (walletId == Guid.Empty)
            throw new ArgumentException("Wallet ID cannot be empty.", nameof(walletId));

        if (amount <= 0)
            throw new ArgumentException("Amount must be positive.", nameof(amount));

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        return new Transaction
        {
            WalletId = walletId,
            Amount = amount,
            Type = type,
            Reason = reason,
            TenantId = tenantId,
            ReferenceId = referenceId
        };
    }
}
