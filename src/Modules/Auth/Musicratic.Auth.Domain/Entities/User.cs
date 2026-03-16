using Musicratic.Auth.Domain.Events;
using Musicratic.Shared.Domain;

namespace Musicratic.Auth.Domain.Entities;

public sealed class User : AuditableEntity
{
    public string AuthentikSub { get; private set; } = string.Empty;

    public string DisplayName { get; private set; } = string.Empty;

    public string Email { get; private set; } = string.Empty;

    public string? AvatarUrl { get; private set; }

    public int WalletBalance { get; private set; }

    private User() { }

    public static User Create(string authentikSub, string displayName, string email, string? avatarUrl = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(authentikSub);
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(email);

        var user = new User
        {
            AuthentikSub = authentikSub,
            DisplayName = displayName,
            Email = email,
            AvatarUrl = avatarUrl,
            WalletBalance = 0
        };

        user.AddDomainEvent(new UserCreatedEvent(user.Id, email));

        return user;
    }

    public void CreditWallet(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Credit amount must be positive.");

        WalletBalance += amount;

        AddDomainEvent(new WalletCreditedEvent(Id, amount, WalletBalance));
    }

    public void DebitWallet(int amount)
    {
        if (amount <= 0)
            throw new ArgumentOutOfRangeException(nameof(amount), "Debit amount must be positive.");

        if (WalletBalance < amount)
            throw new InvalidOperationException($"Insufficient wallet balance. Current: {WalletBalance}, Requested: {amount}.");

        WalletBalance -= amount;

        AddDomainEvent(new WalletDebitedEvent(Id, amount, WalletBalance));
    }

    public void UpdateProfile(string displayName, string? avatarUrl)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        DisplayName = displayName;
        AvatarUrl = avatarUrl;
    }
}
