using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musicratic.Economy.Application;
using Musicratic.Economy.Application.Services;
using Musicratic.Economy.Domain.Repositories;

namespace Musicratic.Economy.Infrastructure.Services;

/// <summary>
/// ECON-009: Apple IAP receipt verification via App Store Server API.
/// Graceful degradation: returns error if not configured.
/// </summary>
public sealed class AppleIapService(
    IWalletRepository walletRepository,
    ICoinPackageRepository coinPackageRepository,
    IEconomyUnitOfWork unitOfWork,
    IOptions<AppleIapOptions> options,
    ILogger<AppleIapService> logger) : IAppleIapService
{
    private static readonly HttpClient HttpClient = new();
    private readonly AppleIapOptions _options = options.Value;

    public async Task<IapVerificationResult> VerifyReceipt(
        string receiptData,
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            logger.LogWarning("Apple IAP is not configured — receipt verification unavailable");
            return new IapVerificationResult(false, 0, "Apple IAP not configured.");
        }

        try
        {
            var verifyUrl = _options.UseSandbox
                ? "https://sandbox.itunes.apple.com/verifyReceipt"
                : "https://buy.itunes.apple.com/verifyReceipt";

            var requestBody = new { receipt_data = receiptData, password = _options.SharedSecret };
            var response = await HttpClient.PostAsJsonAsync(
                verifyUrl, requestBody, cancellationToken);

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Apple IAP verification failed with status {StatusCode}",
                    response.StatusCode);
                return new IapVerificationResult(false, 0, "Apple verification request failed.");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var status = doc.RootElement.GetProperty("status").GetInt32();

            if (status != 0)
            {
                logger.LogWarning("Apple receipt verification returned status {Status}", status);
                return new IapVerificationResult(false, 0, $"Apple receipt invalid (status: {status}).");
            }

            // Extract product ID from the latest receipt info
            var productId = ExtractProductId(doc.RootElement);
            if (productId is null)
            {
                return new IapVerificationResult(false, 0, "Could not extract product from receipt.");
            }

            // Map product to coin package
            var packages = await coinPackageRepository.GetActivePackages(cancellationToken);
            var package = packages.FirstOrDefault(p =>
                p.Name.Equals(productId, StringComparison.OrdinalIgnoreCase));

            if (package is null)
            {
                logger.LogWarning("No coin package found for Apple product {ProductId}", productId);
                return new IapVerificationResult(false, 0, "No matching coin package found.");
            }

            // Credit coins
            var wallet = await walletRepository.GetByUserAndTenant(
                userId, tenantId, cancellationToken);
            if (wallet is null)
            {
                wallet = Domain.Entities.Wallet.Create(userId, tenantId);
                await walletRepository.Add(wallet, cancellationToken);
            }

            var totalCoins = package.CoinAmount + package.BonusCoins;
            wallet.Credit(totalCoins, $"Apple IAP: {package.Name}");
            walletRepository.Update(wallet);
            await unitOfWork.SaveChanges(cancellationToken);

            logger.LogInformation(
                "Credited {Coins} coins to user {UserId} via Apple IAP",
                totalCoins, userId);

            return new IapVerificationResult(true, totalCoins);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Apple IAP verification error for user {UserId}", userId);
            return new IapVerificationResult(false, 0, "Apple IAP verification error.");
        }
    }

    private static string? ExtractProductId(JsonElement root)
    {
        try
        {
            if (root.TryGetProperty("receipt", out var receipt) &&
                receipt.TryGetProperty("in_app", out var inApp) &&
                inApp.GetArrayLength() > 0)
            {
                return inApp[inApp.GetArrayLength() - 1]
                    .GetProperty("product_id").GetString();
            }
        }
        catch
        {
            // Graceful degradation
        }

        return null;
    }
}

public sealed class AppleIapOptions
{
    public const string SectionName = "Economy:AppleIap";

    public string? SharedSecret { get; set; }
    public bool UseSandbox { get; set; } = true;

    public bool IsConfigured => !string.IsNullOrWhiteSpace(SharedSecret);
}
