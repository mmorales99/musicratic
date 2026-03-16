using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using Musicratic.Hub.Application.Services;

namespace Musicratic.Hub.Infrastructure.Services;

public sealed class HubLinkService : IHubLinkService, IDisposable
{
    private const string DefaultBaseUrl = "https://musicratic.app";

    private readonly HMACSHA256 _hmac;
    private readonly string _baseUrl;

    public HubLinkService(IConfiguration configuration)
    {
        var signingKey = configuration["HubSettings:SigningKey"]
            ?? throw new InvalidOperationException("HubSettings:SigningKey is not configured.");

        _hmac = new HMACSHA256(Encoding.UTF8.GetBytes(signingKey));
        _baseUrl = configuration["HubSettings:BaseUrl"] ?? DefaultBaseUrl;
    }

    public string GenerateJoinUrl(string hubCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hubCode);

        var signature = GenerateSignature(hubCode);
        return $"{_baseUrl}/join/{Uri.EscapeDataString(hubCode)}?sig={Uri.EscapeDataString(signature)}";
    }

    public string GenerateSignature(string hubCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hubCode);

        var hash = _hmac.ComputeHash(Encoding.UTF8.GetBytes(hubCode));
        return Base64UrlEncode(hash);
    }

    public bool ValidateSignature(string hubCode, string signature)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hubCode);
        ArgumentException.ThrowIfNullOrWhiteSpace(signature);

        var expected = GenerateSignature(hubCode);
        return CryptographicOperations.FixedTimeEquals(
            Encoding.UTF8.GetBytes(expected),
            Encoding.UTF8.GetBytes(signature));
    }

    public void Dispose()
    {
        _hmac.Dispose();
    }

    private static string Base64UrlEncode(byte[] data)
    {
        return Convert.ToBase64String(data)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }
}
