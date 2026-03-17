using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Musicratic.Economy.Application.Commands.CreateCheckout;
using Musicratic.Economy.Application.Commands.VerifyAppleReceipt;
using Musicratic.Economy.Application.Commands.VerifyGooglePurchase;
using Musicratic.Economy.Application.Queries.GetTransactionHistory;
using Musicratic.Economy.Application.Services;
using Musicratic.Economy.Domain.Enums;
using Musicratic.Economy.Domain.Repositories;

namespace Musicratic.Economy.Api.Endpoints;

/// <summary>
/// ECON-015: Economy module REST endpoints.
/// All endpoints use Problem Details (RFC 9457) for errors.
/// </summary>
public static class EconomyEndpoints
{
    public static RouteGroupBuilder MapEconomyEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api").WithTags("Economy");

        // Wallet
        group.MapGet("/wallet", GetWalletBalance).WithName("GetWalletBalance");
        group.MapGet("/wallet/transactions", GetTransactionHistory)
            .WithName("GetTransactionHistory");

        // Coin packages
        group.MapGet("/coin-packages", GetCoinPackages).WithName("GetCoinPackages");

        // Purchases
        group.MapPost("/purchases/checkout", CreateCheckout).WithName("CreateCheckout");
        group.MapPost("/purchases/verify-apple", VerifyAppleReceipt)
            .WithName("VerifyAppleReceipt");
        group.MapPost("/purchases/verify-google", VerifyGooglePurchase)
            .WithName("VerifyGooglePurchase");

        // Subscriptions
        group.MapGet("/subscriptions/{hubId:guid}", GetSubscription)
            .WithName("GetSubscription");
        group.MapPost("/subscriptions/{hubId:guid}/trial", StartFreeTrial)
            .WithName("StartFreeTrial");

        // Pricing
        group.MapGet("/pricing/track", CalculateTrackPrice).WithName("CalculateTrackPrice");

        return group;
    }

    private static async Task<IResult> GetWalletBalance(
        Guid userId,
        Guid tenantId,
        IWalletRepository walletRepository,
        CancellationToken cancellationToken)
    {
        var wallet = await walletRepository.GetByUserAndTenant(
            userId, tenantId, cancellationToken);

        if (wallet is null)
        {
            return Results.Problem(
                detail: "Wallet not found for the specified user.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Wallet Not Found");
        }

        return Results.Ok(new
        {
            success = true,
            data = new
            {
                walletId = wallet.Id,
                balance = wallet.Balance,
                currency = wallet.Currency
            }
        });
    }

    private static async Task<IResult> GetTransactionHistory(
        Guid userId,
        Guid tenantId,
        int page,
        int pageSize,
        TransactionType? type,
        ISender sender,
        CancellationToken cancellationToken)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var query = new GetTransactionHistoryQuery(userId, tenantId, page, pageSize, type);
        var result = await sender.Send(query, cancellationToken);

        return Results.Ok(new
        {
            success = true,
            total_items_in_response = result.Items.Count,
            has_more_items = result.HasMoreItems,
            items = result.Items
        });
    }

    private static async Task<IResult> GetCoinPackages(
        ICoinPackageRepository coinPackageRepository,
        CancellationToken cancellationToken)
    {
        var packages = await coinPackageRepository.GetActivePackages(cancellationToken);

        return Results.Ok(new
        {
            success = true,
            total_items_in_response = packages.Count,
            has_more_items = false,
            items = packages.Select(p => new
            {
                id = p.Id,
                name = p.Name,
                coinAmount = p.CoinAmount,
                bonusCoins = p.BonusCoins,
                priceUsd = p.PriceUsd
            })
        });
    }

    private static async Task<IResult> CreateCheckout(
        CreateCheckoutRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new CreateCheckoutCommand(
            request.UserId, request.CoinPackageId, request.TenantId);

        var result = await sender.Send(command, cancellationToken);

        if (!result.Success)
        {
            return Results.Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Checkout Creation Failed");
        }

        return Results.Ok(new { success = true, sessionId = result.SessionId, url = result.Url });
    }

    private static async Task<IResult> VerifyAppleReceipt(
        VerifyAppleReceiptRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new VerifyAppleReceiptCommand(
            request.ReceiptData, request.UserId, request.TenantId);

        var result = await sender.Send(command, cancellationToken);

        if (!result.Success)
        {
            return Results.Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Apple Receipt Verification Failed");
        }

        return Results.Ok(new
        {
            success = true,
            coinsAwarded = result.CoinsAwarded
        });
    }

    private static async Task<IResult> VerifyGooglePurchase(
        VerifyGooglePurchaseRequest request,
        ISender sender,
        CancellationToken cancellationToken)
    {
        var command = new VerifyGooglePurchaseCommand(
            request.PurchaseToken, request.ProductId, request.UserId, request.TenantId);

        var result = await sender.Send(command, cancellationToken);

        if (!result.Success)
        {
            return Results.Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Google Purchase Verification Failed");
        }

        return Results.Ok(new
        {
            success = true,
            coinsAwarded = result.CoinsAwarded
        });
    }

    private static async Task<IResult> GetSubscription(
        Guid hubId,
        ISubscriptionRepository subscriptionRepository,
        CancellationToken cancellationToken)
    {
        var subscription = await subscriptionRepository.GetByHubId(
            hubId, cancellationToken);

        if (subscription is null)
        {
            return Results.Problem(
                detail: "No subscription found for this hub.",
                statusCode: StatusCodes.Status404NotFound,
                title: "Subscription Not Found");
        }

        return Results.Ok(new
        {
            success = true,
            data = new
            {
                id = subscription.Id,
                hubId = subscription.HubId,
                tier = subscription.Tier.ToString(),
                startedAt = subscription.StartedAt,
                expiresAt = subscription.ExpiresAt,
                trialEndsAt = subscription.TrialEndsAt,
                isActive = subscription.IsActive,
                isTrialActive = subscription.IsTrialActive,
                trialDaysRemaining = subscription.TrialDaysRemaining
            }
        });
    }

    private static async Task<IResult> StartFreeTrial(
        Guid hubId,
        StartFreeTrialRequest request,
        IFreeTrialService freeTrialService,
        CancellationToken cancellationToken)
    {
        var result = await freeTrialService.StartTrial(
            hubId, request.TenantId, cancellationToken);

        if (!result.Success)
        {
            return Results.Problem(
                detail: result.ErrorMessage,
                statusCode: StatusCodes.Status400BadRequest,
                title: "Free Trial Failed");
        }

        return Results.Ok(new
        {
            success = true,
            trialEndsAt = result.TrialEndsAt
        });
    }

    private static IResult CalculateTrackPrice(
        int durationSeconds,
        double hotnessScore,
        ICoinPricingEngine pricingEngine)
    {
        var price = pricingEngine.CalculatePrice(durationSeconds, hotnessScore);

        return Results.Ok(new
        {
            success = true,
            data = new
            {
                baseCost = price.BaseCost,
                multiplier = price.Multiplier,
                finalCost = price.FinalCost
            }
        });
    }
}

// Request DTOs
public sealed record CreateCheckoutRequest(Guid UserId, Guid CoinPackageId, Guid TenantId);
public sealed record VerifyAppleReceiptRequest(string ReceiptData, Guid UserId, Guid TenantId);
public sealed record VerifyGooglePurchaseRequest(
    string PurchaseToken, string ProductId, Guid UserId, Guid TenantId);
public sealed record StartFreeTrialRequest(Guid TenantId);
