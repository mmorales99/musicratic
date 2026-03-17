using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musicratic.Economy.Application.Services;
using Musicratic.Economy.Domain.Enums;
using Musicratic.Economy.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Economy.Infrastructure.Services;

/// <summary>
/// ECON-007: Stripe webhook handler — verifies signature and processes checkout.session.completed.
/// Idempotent: checks if transaction with same reference_id already exists.
/// </summary>
public sealed class StripeWebhookHandler(
    IWalletRepository walletRepository,
    ITransactionRepository transactionRepository,
    ICoinPackageRepository coinPackageRepository,
    IUnitOfWork unitOfWork,
    IOptions<StripeOptions> stripeOptions,
    ILogger<StripeWebhookHandler> logger) : IStripeWebhookHandler
{
    private readonly StripeOptions _options = stripeOptions.Value;

    public async Task<WebhookResult> HandleCheckoutCompleted(
        string payload,
        string signature,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
        {
            logger.LogError("Stripe webhook secret is not configured");
            return new WebhookResult(false, "Stripe webhook secret not configured.");
        }

        if (!VerifySignature(payload, signature, _options.WebhookSecret))
        {
            logger.LogWarning("Invalid Stripe webhook signature");
            return new WebhookResult(false, "Invalid webhook signature.");
        }

        var eventData = ParseCheckoutEvent(payload);
        if (eventData is null)
        {
            return new WebhookResult(false, "Unable to parse webhook event.");
        }

        // Idempotency: check if already processed
        var existing = await transactionRepository.Find(
            t => t.ReferenceId == eventData.SessionId, cancellationToken);
        if (existing.Count > 0)
        {
            logger.LogInformation(
                "Webhook already processed for session {SessionId}", eventData.SessionId);
            return new WebhookResult(true);
        }

        var package = await coinPackageRepository.GetById(
            eventData.CoinPackageId, cancellationToken);
        if (package is null)
        {
            logger.LogWarning("Coin package {PackageId} not found", eventData.CoinPackageId);
            return new WebhookResult(false, "Coin package not found.");
        }

        var wallet = await walletRepository.GetByUserAndTenant(
            eventData.UserId, eventData.TenantId, cancellationToken);

        if (wallet is null)
        {
            wallet = Domain.Entities.Wallet.Create(eventData.UserId, eventData.TenantId);
            await walletRepository.Add(wallet, cancellationToken);
        }

        var totalCoins = package.CoinAmount + package.BonusCoins;
        var reason = $"Purchase: {package.Name} ({package.CoinAmount} + {package.BonusCoins} bonus)";
        wallet.Credit(totalCoins, reason, eventData.SessionId);

        walletRepository.Update(wallet);
        await unitOfWork.SaveChanges(cancellationToken);

        logger.LogInformation(
            "Credited {Coins} coins to user {UserId} for Stripe session {SessionId}",
            totalCoins, eventData.UserId, eventData.SessionId);

        return new WebhookResult(true);
    }

    internal static bool VerifySignature(
        string payload, string signatureHeader, string secret)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader))
            return false;

        // Parse Stripe signature format: t=timestamp,v1=signature
        var parts = signatureHeader.Split(',');
        string? timestamp = null;
        string? expectedSig = null;

        foreach (var part in parts)
        {
            var kv = part.Split('=', 2);
            if (kv.Length != 2) continue;
            if (kv[0] == "t") timestamp = kv[1];
            if (kv[0] == "v1") expectedSig = kv[1];
        }

        if (timestamp is null || expectedSig is null) return false;

        var signedPayload = $"{timestamp}.{payload}";
        var secretBytes = Encoding.UTF8.GetBytes(secret);
        var payloadBytes = Encoding.UTF8.GetBytes(signedPayload);

        using var hmac = new HMACSHA256(secretBytes);
        var hash = hmac.ComputeHash(payloadBytes);
        var computedSig = Convert.ToHexString(hash).ToLowerInvariant();

        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(computedSig),
            Encoding.UTF8.GetBytes(expectedSig));
    }

    private static CheckoutEventData? ParseCheckoutEvent(string payload)
    {
        try
        {
            using var doc = JsonDocument.Parse(payload);
            var root = doc.RootElement;

            var eventType = root.GetProperty("type").GetString();
            if (eventType != "checkout.session.completed") return null;

            var session = root.GetProperty("data").GetProperty("object");
            var metadata = session.GetProperty("metadata");

            var sessionId = session.GetProperty("id").GetString();
            var userId = Guid.Parse(metadata.GetProperty("userId").GetString()!);
            var packageId = Guid.Parse(metadata.GetProperty("coinPackageId").GetString()!);
            var tenantId = Guid.Parse(metadata.GetProperty("tenantId").GetString()!);

            return new CheckoutEventData(Guid.Parse(sessionId!), userId, packageId, tenantId);
        }
        catch
        {
            return null;
        }
    }
}

internal sealed record CheckoutEventData(
    Guid SessionId,
    Guid UserId,
    Guid CoinPackageId,
    Guid TenantId);
