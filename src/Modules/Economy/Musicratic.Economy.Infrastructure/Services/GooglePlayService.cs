using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musicratic.Economy.Application.Services;
using Musicratic.Economy.Domain.Repositories;
using Musicratic.Shared.Application;

namespace Musicratic.Economy.Infrastructure.Services;

/// <summary>
/// ECON-010: Google Play billing verification via androidpublisher/v3 API.
/// Uses service account auth. Graceful degradation if not configured.
/// </summary>
public sealed class GooglePlayService(
    IWalletRepository walletRepository,
    ICoinPackageRepository coinPackageRepository,
    IUnitOfWork unitOfWork,
    IOptions<GooglePlayOptions> options,
    ILogger<GooglePlayService> logger) : IGooglePlayService
{
    private static readonly HttpClient HttpClient = new();
    private readonly GooglePlayOptions _options = options.Value;

    public async Task<PurchaseVerificationResult> VerifyPurchase(
        string purchaseToken,
        string productId,
        Guid userId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (!_options.IsConfigured)
        {
            logger.LogWarning("Google Play billing is not configured");
            return new PurchaseVerificationResult(false, 0, "Google Play not configured.");
        }

        try
        {
            var url = $"https://androidpublisher.googleapis.com/androidpublisher/v3/" +
                      $"applications/{_options.PackageName}/purchases/products/{productId}/" +
                      $"tokens/{purchaseToken}";

            var request = new HttpRequestMessage(HttpMethod.Get, url);
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", _options.ServiceAccountToken);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Google Play API error {StatusCode}: {Body}",
                    response.StatusCode, responseBody);
                return new PurchaseVerificationResult(false, 0, "Google Play verification failed.");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var purchaseState = doc.RootElement.GetProperty("purchaseState").GetInt32();

            // 0 = purchased
            if (purchaseState != 0)
            {
                return new PurchaseVerificationResult(
                    false, 0, $"Purchase not completed (state: {purchaseState}).");
            }

            // Map product to coin package
            var packages = await coinPackageRepository.GetActivePackages(cancellationToken);
            var package = packages.FirstOrDefault(p =>
                p.Name.Equals(productId, StringComparison.OrdinalIgnoreCase));

            if (package is null)
            {
                logger.LogWarning("No coin package found for Google product {ProductId}", productId);
                return new PurchaseVerificationResult(false, 0, "No matching coin package found.");
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
            wallet.Credit(totalCoins, $"Google Play: {package.Name}");
            walletRepository.Update(wallet);

            // Acknowledge the purchase
            await AcknowledgePurchase(productId, purchaseToken, cancellationToken);

            await unitOfWork.SaveChanges(cancellationToken);

            logger.LogInformation(
                "Credited {Coins} coins to user {UserId} via Google Play",
                totalCoins, userId);

            return new PurchaseVerificationResult(true, totalCoins);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Google Play verification error for user {UserId}", userId);
            return new PurchaseVerificationResult(false, 0, "Google Play verification error.");
        }
    }

    private async Task AcknowledgePurchase(
        string productId, string purchaseToken,
        CancellationToken cancellationToken)
    {
        try
        {
            var url = $"https://androidpublisher.googleapis.com/androidpublisher/v3/" +
                      $"applications/{_options.PackageName}/purchases/products/{productId}/" +
                      $"tokens/{purchaseToken}:acknowledge";

            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Content = new StringContent("{}", System.Text.Encoding.UTF8, "application/json")
            };
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue(
                    "Bearer", _options.ServiceAccountToken);

            await HttpClient.SendAsync(request, cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to acknowledge Google Play purchase for {ProductId}", productId);
        }
    }
}

public sealed class GooglePlayOptions
{
    public const string SectionName = "Economy:GooglePlay";

    public string? PackageName { get; set; }
    public string? ServiceAccountToken { get; set; }

    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(PackageName) &&
        !string.IsNullOrWhiteSpace(ServiceAccountToken);
}
