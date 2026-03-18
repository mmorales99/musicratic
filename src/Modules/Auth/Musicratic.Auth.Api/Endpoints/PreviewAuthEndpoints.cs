using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Musicratic.Auth.Api.Endpoints;

public static class PreviewAuthEndpoints
{
    public static IEndpointRouteBuilder MapPreviewAuthEndpoints(
        this IEndpointRouteBuilder app,
        IConfiguration configuration)
    {
        var previewEnabled = configuration.GetValue<bool>("Preview:Enabled");
        if (!previewEnabled) return app;

        var signingKey = configuration["Preview:JwtSigningKey"] ?? string.Empty;
        if (signingKey.Length < 32) return app;

        var group = app.MapGroup("/api/preview/auth")
            .WithTags("Preview Auth")
            .AllowAnonymous();

        group.MapPost("/token", (PreviewTokenRequest request) =>
        {
            var keyBytes = Encoding.UTF8.GetBytes(signingKey);
            var securityKey = new SymmetricSecurityKey(keyBytes);
            var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, request.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, request.Email ?? $"user-{request.UserId:N}@preview.musicratic.app"),
                new Claim("name", request.DisplayName ?? $"Preview User {request.UserId:N}"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: "musicratic-preview",
                audience: "musicratic-api",
                claims: claims,
                expires: DateTime.UtcNow.AddHours(24),
                signingCredentials: credentials);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            return Results.Ok(new PreviewTokenResponse(tokenString, token.ValidTo));
        })
        .WithName("GetPreviewToken")
        .WithSummary("Issue a preview JWT for testing (only available in Preview mode)");

        return app;
    }

    public sealed record PreviewTokenRequest(
        Guid UserId,
        string? Email = null,
        string? DisplayName = null);

    public sealed record PreviewTokenResponse(string AccessToken, DateTime ExpiresAt);
}
