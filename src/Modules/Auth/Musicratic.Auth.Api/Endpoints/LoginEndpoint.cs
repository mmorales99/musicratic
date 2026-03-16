using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Musicratic.Auth.Application.Services;
using Musicratic.Auth.Infrastructure.Configuration;

namespace Musicratic.Auth.Api.Endpoints;

public static class LoginEndpoint
{
    private const string CookieName = "musicratic_auth_state";
    private const string ProtectorPurpose = "MusicraticAuth";
    private const int CookieExpirationMinutes = 10;

    public static async Task<IResult> Login(
        IOidcDiscoveryService discoveryService,
        IOptions<AuthentikOptions> options,
        [FromServices] IDataProtectionProvider dataProtectionProvider,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var config = options.Value;

        var codeVerifier = GenerateCodeVerifier();
        var codeChallenge = GenerateCodeChallenge(codeVerifier);
        var state = GenerateState();

        var authorizationEndpoint = await discoveryService
            .GetAuthorizationEndpointAsync(cancellationToken);

        var authorizationUrl = BuildAuthorizationUrl(
            authorizationEndpoint, config, state, codeChallenge);

        StoreAuthStateCookie(
            httpContext, dataProtectionProvider, state, codeVerifier);

        return Results.Redirect(authorizationUrl);
    }

    private static string GenerateCodeVerifier()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string GenerateCodeChallenge(string codeVerifier)
    {
        return Convert.ToBase64String(
                SHA256.HashData(Encoding.ASCII.GetBytes(codeVerifier)))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string GenerateState()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
    }

    private static string BuildAuthorizationUrl(
        string authorizationEndpoint,
        AuthentikOptions config,
        string state,
        string codeChallenge)
    {
        var scope = string.Join(" ", config.Scopes);

        var queryParams = new Dictionary<string, string?>
        {
            ["response_type"] = "code",
            ["client_id"] = config.ClientId,
            ["redirect_uri"] = config.RedirectUri,
            ["scope"] = scope,
            ["state"] = state,
            ["code_challenge"] = codeChallenge,
            ["code_challenge_method"] = "S256"
        };

        var queryString = string.Join("&", queryParams
            .Select(kvp => $"{Uri.EscapeDataString(kvp.Key)}={Uri.EscapeDataString(kvp.Value!)}"));

        return $"{authorizationEndpoint}?{queryString}";
    }

    private static void StoreAuthStateCookie(
        HttpContext httpContext,
        IDataProtectionProvider dataProtectionProvider,
        string state,
        string codeVerifier)
    {
        var protector = dataProtectionProvider.CreateProtector(ProtectorPurpose);
        var cookieValue = protector.Protect($"{state}|{codeVerifier}");

        httpContext.Response.Cookies.Append(CookieName, cookieValue, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Lax,
            IsEssential = true,
            MaxAge = TimeSpan.FromMinutes(CookieExpirationMinutes)
        });
    }
}
