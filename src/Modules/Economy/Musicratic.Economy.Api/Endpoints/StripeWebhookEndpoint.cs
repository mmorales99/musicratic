using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Economy.Application.Services;

namespace Musicratic.Economy.Api.Endpoints;

/// <summary>
/// ECON-007: Stripe webhook endpoint.
/// POST /api/webhooks/stripe — raw body for signature verification.
/// </summary>
public static class StripeWebhookEndpoint
{
    public static RouteGroupBuilder MapStripeWebhookEndpoints(
        this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/webhooks").WithTags("Webhooks");

        group.MapPost("/stripe", HandleStripeWebhook)
            .WithName("StripeWebhook")
            .DisableAntiforgery();

        return group;
    }

    private static async Task<IResult> HandleStripeWebhook(
        HttpContext httpContext,
        IStripeWebhookHandler webhookHandler,
        CancellationToken cancellationToken)
    {
        using var reader = new StreamReader(httpContext.Request.Body);
        var payload = await reader.ReadToEndAsync(cancellationToken);

        var signature = httpContext.Request.Headers["Stripe-Signature"].FirstOrDefault() ?? "";

        var result = await webhookHandler.HandleCheckoutCompleted(
            payload, signature, cancellationToken);

        if (!result.Success)
        {
            return Results.Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Webhook Processing Failed");
        }

        return Results.Ok();
    }
}
