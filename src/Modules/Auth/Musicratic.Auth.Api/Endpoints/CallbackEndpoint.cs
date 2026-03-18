using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Musicratic.Auth.Application.Services;
using Musicratic.Auth.Domain.Repositories;
using Musicratic.Auth.Domain.Entities;
using Musicratic.Auth.Infrastructure.Configuration;
using Musicratic.Auth.Application;

namespace Musicratic.Auth.Api.Endpoints;

public static class CallbackEndpoint
{
    private const string CookieName = "musicratic_auth_state";
    private const string ProtectorPurpose = "MusicraticAuth";

    public static async Task<IResult> Callback(
        IOidcDiscoveryService discoveryService,
        IOptions<AuthentikOptions> options,
        [FromServices] IDataProtectionProvider dataProtectionProvider,
        IUserRepository userRepository,
        IAuthUnitOfWork unitOfWork,
        IHttpClientFactory httpClientFactory,
        HttpContext httpContext,
        [FromQuery] string code,
        [FromQuery] string state,
        CancellationToken cancellationToken)
    {
        var (cookieState, codeVerifier) = ReadAndDeleteAuthCookie(
            httpContext, dataProtectionProvider);

        if (cookieState is null)
        {
            return Results.Problem(
                title: "Missing auth state cookie",
                detail: "The authentication state cookie was not found or could not be decrypted.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        if (!string.Equals(cookieState, state, StringComparison.Ordinal))
        {
            return Results.Problem(
                title: "State mismatch",
                detail: "The state parameter does not match the stored authentication state.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var config = options.Value;
        var tokenEndpoint = await discoveryService.GetTokenEndpointAsync(cancellationToken);

        var tokenResponse = await ExchangeCodeForTokens(
            httpClientFactory, tokenEndpoint, config, code, codeVerifier!, cancellationToken);

        if (tokenResponse is null)
        {
            return Results.Problem(
                title: "Token exchange failed",
                detail: "Failed to exchange authorization code for tokens with the identity provider.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        var userInfoEndpoint = await discoveryService.GetUserInfoEndpointAsync(cancellationToken);

        var userInfo = await FetchUserInfo(
            httpClientFactory, userInfoEndpoint, tokenResponse.AccessToken, cancellationToken);

        if (userInfo is null)
        {
            return Results.Problem(
                title: "UserInfo request failed",
                detail: "Failed to retrieve user information from the identity provider.",
                statusCode: StatusCodes.Status502BadGateway);
        }

        await UpsertUser(userRepository, unitOfWork, userInfo, cancellationToken);

        return Results.Ok(new
        {
            access_token = tokenResponse.AccessToken,
            refresh_token = tokenResponse.RefreshToken,
            expires_in = tokenResponse.ExpiresIn
        });
    }

    private static (string? State, string? CodeVerifier) ReadAndDeleteAuthCookie(
        HttpContext httpContext,
        IDataProtectionProvider dataProtectionProvider)
    {
        if (!httpContext.Request.Cookies.TryGetValue(CookieName, out var cookieValue)
            || string.IsNullOrEmpty(cookieValue))
        {
            return (null, null);
        }

        httpContext.Response.Cookies.Delete(CookieName);

        try
        {
            var protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
            var decrypted = protector.Unprotect(cookieValue);
            var parts = decrypted.Split('|', 2);

            if (parts.Length != 2)
                return (null, null);

            return (parts[0], parts[1]);
        }
        catch
        {
            return (null, null);
        }
    }

    private static async Task<TokenResponse?> ExchangeCodeForTokens(
        IHttpClientFactory httpClientFactory,
        string tokenEndpoint,
        AuthentikOptions config,
        string code,
        string codeVerifier,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Authentik");

        var parameters = new Dictionary<string, string>
        {
            ["grant_type"] = "authorization_code",
            ["code"] = code,
            ["redirect_uri"] = config.RedirectUri,
            ["client_id"] = config.ClientId,
            ["client_secret"] = config.ClientSecret,
            ["code_verifier"] = codeVerifier
        };

        using var content = new FormUrlEncodedContent(parameters);

        try
        {
            using var response = await client.PostAsync(tokenEndpoint, content, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<TokenResponse>(cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private static async Task<UserInfoResponse?> FetchUserInfo(
        IHttpClientFactory httpClientFactory,
        string userInfoEndpoint,
        string accessToken,
        CancellationToken cancellationToken)
    {
        var client = httpClientFactory.CreateClient("Authentik");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", accessToken);

        try
        {
            using var response = await client.GetAsync(userInfoEndpoint, cancellationToken);

            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<UserInfoResponse>(cancellationToken);
        }
        catch
        {
            return null;
        }
    }

    private static async Task UpsertUser(
        IUserRepository userRepository,
        IAuthUnitOfWork unitOfWork,
        UserInfoResponse userInfo,
        CancellationToken cancellationToken)
    {
        var displayName = userInfo.PreferredUsername ?? userInfo.Name ?? "Unknown";
        var existingUser = await userRepository.GetByAuthentikSub(userInfo.Sub, cancellationToken);

        if (existingUser is not null)
        {
            existingUser.UpdateProfile(displayName, userInfo.Picture);
        }
        else
        {
            var newUser = User.Create(userInfo.Sub, displayName, userInfo.Email ?? "", userInfo.Picture);
            await userRepository.Add(newUser, cancellationToken);
        }

        await unitOfWork.SaveChanges(cancellationToken);
    }

    private sealed record TokenResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("refresh_token")] string RefreshToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);

    private sealed record UserInfoResponse(
        [property: JsonPropertyName("sub")] string Sub,
        [property: JsonPropertyName("preferred_username")] string? PreferredUsername,
        [property: JsonPropertyName("name")] string? Name,
        [property: JsonPropertyName("email")] string? Email,
        [property: JsonPropertyName("picture")] string? Picture);
}
