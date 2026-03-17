namespace Musicratic.Economy.Infrastructure.Services;

/// <summary>
/// Stripe configuration options.
/// </summary>
public sealed class StripeOptions
{
    public const string SectionName = "Economy:Stripe";

    public string? ApiKey { get; set; }
    public string? WebhookSecret { get; set; }
    public string? SuccessUrl { get; set; } = "/payment/success";
    public string? CancelUrl { get; set; } = "/payment/cancel";
}
