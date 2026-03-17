namespace Musicratic.Shared.Contracts.DTOs;

public sealed record WalletDebitResult(
    bool Success,
    decimal NewBalance,
    string? ErrorMessage);
