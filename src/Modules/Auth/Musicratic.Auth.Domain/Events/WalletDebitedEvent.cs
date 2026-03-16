using Musicratic.Shared.Domain;

namespace Musicratic.Auth.Domain.Events;

public sealed record WalletDebitedEvent(Guid UserId, int Amount, int NewBalance) : DomainEvent;
