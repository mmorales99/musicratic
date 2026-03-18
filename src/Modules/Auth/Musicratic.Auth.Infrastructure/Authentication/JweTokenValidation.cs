using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using Musicratic.Auth.Infrastructure.Configuration;

namespace Musicratic.Auth.Infrastructure.Authentication;

public static class JweTokenValidation
{
    public static IServiceCollection AddAuthentikAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var previewEnabled = configuration.GetValue<bool>("Preview:Enabled");
        var previewKey = configuration["Preview:JwtSigningKey"] ?? string.Empty;

        if (previewEnabled && previewKey.Length >= 32)
        {
            return AddPreviewAuthentication(services, previewKey);
        }

        return AddAuthentikOidcAuthentication(services, configuration);
    }

    private static IServiceCollection AddAuthentikOidcAuthentication(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var authentikOptions = configuration
            .GetSection(AuthentikOptions.SectionName)
            .Get<AuthentikOptions>() ?? new AuthentikOptions();

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.Authority = authentikOptions.Authority;
                options.RequireHttpsMetadata = true;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidIssuer = authentikOptions.Authority,
                    ValidAudience = authentikOptions.ClientId
                };
            });

        return services;
    }

    private static IServiceCollection AddPreviewAuthentication(
        IServiceCollection services,
        string signingKey)
    {
        var keyBytes = Encoding.UTF8.GetBytes(signingKey);
        var securityKey = new SymmetricSecurityKey(keyBytes);

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;

                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = "musicratic-preview",
                    ValidAudience = "musicratic-api",
                    IssuerSigningKey = securityKey
                };
            });

        return services;
    }
}
