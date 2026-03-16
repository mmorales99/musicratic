using System.Security.Cryptography;
using Musicratic.Hub.Domain.Repositories;
using Musicratic.Hub.Domain.Services;

namespace Musicratic.Hub.Infrastructure.Services;

public sealed class HubCodeGenerator(IHubRepository hubRepository) : IHubCodeGenerator
{
    private const int MaxRetries = 10;
    private const int MinCodeLength = 8;
    private const int MaxPrefixLength = 8;
    private const int SuffixLength = 2;

    public async Task<string> Generate(string hubName, CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(hubName);

        var prefix = SanitizePrefix(hubName);

        for (var attempt = 0; attempt < MaxRetries; attempt++)
        {
            var suffix = RandomNumberGenerator.GetInt32(10, 100).ToString();
            var code = $"{prefix}{suffix}";

            var existing = await hubRepository.GetByCode(code, cancellationToken);
            if (existing is null)
                return code;
        }

        throw new InvalidOperationException(
            $"Failed to generate a unique hub code after {MaxRetries} attempts for hub name '{hubName}'.");
    }

    private static string SanitizePrefix(string name)
    {
        var sanitized = new string(name.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();

        if (sanitized.Length == 0)
            sanitized = "HUB";

        var targetPrefixLength = Math.Min(sanitized.Length, MaxPrefixLength);

        // Ensure total code length is at least MinCodeLength
        if (targetPrefixLength + SuffixLength < MinCodeLength)
            targetPrefixLength = Math.Min(sanitized.Length, MinCodeLength - SuffixLength);

        return sanitized[..Math.Min(targetPrefixLength, sanitized.Length)];
    }
}
