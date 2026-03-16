using Musicratic.Shared.Domain;

namespace Musicratic.Auth.Domain.Events;

public sealed record WalletCreditedEvent(Guid UserId, int Amount, int NewBalance) : DomainEvent;
