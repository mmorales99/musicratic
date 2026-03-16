using Musicratic.Economy.Domain.Enums;
using Musicratic.Shared.Domain;

namespace Musicratic.Economy.Domain.Entities;

public sealed class Wallet : BaseEntity, ITenantScoped
{
    public Guid TenantId { get; private set; }

    public Guid UserId { get; private set; }

    public decimal Balance { get; private set; }

    public string Currency { get; private set; } = "MUS_COIN";

    private readonly List<Transaction> _transactions = [];

    public IReadOnlyCollection<Transaction> Transactions => _transactions.AsReadOnly();

    private Wallet() { }

    public static Wallet Create(Guid userId, Guid tenantId)
    {
        if (userId == Guid.Empty)
            throw new ArgumentException("User ID cannot be empty.", nameof(userId));

        if (tenantId == Guid.Empty)
            throw new ArgumentException("Tenant ID cannot be empty.", nameof(tenantId));

        return new Wallet
        {
            UserId = userId,
            TenantId = tenantId,
            Balance = 0m,
            Currency = "MUS_COIN"
        };
    }

    /// <summary>
    /// Credits the wallet and records a transaction.
    /// </summary>
    public Transaction Credit(decimal amount, string reason, Guid? referenceId = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Credit amount must be positive.", nameof(amount));

        Balance += amount;

        var transaction = Transaction.Create(
            Id, amount, TransactionType.Credit, reason, TenantId, referenceId);
        _transactions.Add(transaction);

        return transaction;
    }

    /// <summary>
    /// Debits the wallet and records a transaction.
    /// Validates sufficient balance before debiting.
    /// </summary>
    public Transaction Debit(decimal amount, string reason, Guid? referenceId = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Debit amount must be positive.", nameof(amount));

        if (Balance < amount)
            throw new InvalidOperationException(
                $"Insufficient balance. Current: {Balance}, Requested: {amount}.");

        Balance -= amount;

        var transaction = Transaction.Create(
            Id, amount, TransactionType.Debit, reason, TenantId, referenceId);
        _transactions.Add(transaction);

        return transaction;
    }

    /// <summary>
    /// Processes a refund and records a Refund transaction.
    /// </summary>
    public Transaction Refund(decimal amount, string reason, Guid? referenceId = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Refund amount must be positive.", nameof(amount));

        Balance += amount;

        var transaction = Transaction.Create(
            Id, amount, TransactionType.Refund, reason, TenantId, referenceId);
        _transactions.Add(transaction);

        return transaction;
    }
}
