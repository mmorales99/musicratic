using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Musicratic.Economy.Application.Services;
using Musicratic.Economy.Domain.Repositories;

namespace Musicratic.Economy.Infrastructure.Services;

/// <summary>
/// ECON-008: Creates Stripe Checkout Sessions via the Stripe API.
/// Graceful degradation: returns error result if API key is not configured.
/// </summary>
public sealed class StripeService(
    ICoinPackageRepository coinPackageRepository,
    IOptions<StripeOptions> stripeOptions,
    ILogger<StripeService> logger) : IStripeService
{
    private static readonly HttpClient HttpClient = new();
    private readonly StripeOptions _options = stripeOptions.Value;

    public async Task<CheckoutSessionResult> CreateCheckoutSession(
        Guid userId,
        Guid coinPackageId,
        Guid tenantId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            logger.LogWarning("Stripe API key not configured — cannot create checkout session");
            return new CheckoutSessionResult("", "", false, "Stripe API key not configured.");
        }

        var package = await coinPackageRepository.GetById(coinPackageId, cancellationToken);
        if (package is null)
        {
            return new CheckoutSessionResult("", "", false, "Coin package not found.");
        }

        try
        {
            var formData = new Dictionary<string, string>
            {
                ["mode"] = "payment",
                ["success_url"] = _options.SuccessUrl ?? "/payment/success",
                ["cancel_url"] = _options.CancelUrl ?? "/payment/cancel",
                ["line_items[0][price_data][currency]"] = "eur",
                ["line_items[0][price_data][unit_amount]"] = ((int)(package.PriceUsd * 100)).ToString(),
                ["line_items[0][price_data][product_data][name]"] = package.Name,
                ["line_items[0][quantity]"] = "1",
                ["metadata[userId]"] = userId.ToString(),
                ["metadata[coinPackageId]"] = coinPackageId.ToString(),
                ["metadata[tenantId]"] = tenantId.ToString()
            };

            var request = new HttpRequestMessage(
                HttpMethod.Post, "https://api.stripe.com/v1/checkout/sessions")
            {
                Content = new FormUrlEncodedContent(formData)
            };
            request.Headers.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _options.ApiKey);

            var response = await HttpClient.SendAsync(request, cancellationToken);
            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                logger.LogWarning(
                    "Stripe API error {StatusCode}: {Body}",
                    response.StatusCode, responseBody);
                return new CheckoutSessionResult("", "", false, "Stripe API error.");
            }

            using var doc = JsonDocument.Parse(responseBody);
            var sessionId = doc.RootElement.GetProperty("id").GetString() ?? "";
            var url = doc.RootElement.GetProperty("url").GetString() ?? "";

            logger.LogInformation(
                "Created Stripe checkout session {SessionId} for user {UserId}",
                sessionId, userId);

            return new CheckoutSessionResult(sessionId, url, true);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to create Stripe checkout session for user {UserId}", userId);
            return new CheckoutSessionResult("", "", false, "Failed to create checkout session.");
        }
    }
}
