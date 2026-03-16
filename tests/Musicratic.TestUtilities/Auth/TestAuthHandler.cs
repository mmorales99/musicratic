using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Musicratic.TestUtilities.Auth;

public static class TestAuthDefaults
{
    public const string AuthenticationScheme = "TestScheme";
}

public sealed class TestAuthHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    public const string UserIdHeader = "X-Test-UserId";
    public const string RoleHeader = "X-Test-UserRole";

    public static readonly Guid DefaultUserId =
        Guid.Parse("aaaaaaaa-bbbb-cccc-dddd-eeeeeeeeee01");

    public const string DefaultRole = "user";
    public const string DefaultEmail = "testuser@example.com";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var userId = Request.Headers.TryGetValue(UserIdHeader, out var uid)
            ? uid.ToString()
            : DefaultUserId.ToString();

        var role = Request.Headers.TryGetValue(RoleHeader, out var r)
            ? r.ToString()
            : DefaultRole;

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim("sub", userId),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Email, DefaultEmail),
        };

        var identity = new ClaimsIdentity(claims, TestAuthDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, TestAuthDefaults.AuthenticationScheme);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}

public static class TestAuthExtensions
{
    public static AuthenticationBuilder AddTestAuthentication(this IServiceCollection services)
    {
        return services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = TestAuthDefaults.AuthenticationScheme;
                options.DefaultScheme = TestAuthDefaults.AuthenticationScheme;
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthDefaults.AuthenticationScheme, _ => { });
    }
}
