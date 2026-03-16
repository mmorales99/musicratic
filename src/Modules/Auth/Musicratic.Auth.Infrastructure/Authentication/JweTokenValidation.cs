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
}
