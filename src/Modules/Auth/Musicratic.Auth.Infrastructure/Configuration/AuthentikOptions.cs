namespace Musicratic.Auth.Infrastructure.Configuration;

public sealed class AuthentikOptions
{
    public const string SectionName = "Authentik";

    public string Authority { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string RedirectUri { get; set; } = string.Empty;

    public string PostLogoutRedirectUri { get; set; } = string.Empty;

    public List<string> Scopes { get; set; } = ["openid", "profile", "email"];
}
