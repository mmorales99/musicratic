namespace Musicratic.Auth.Application.Services;

public interface IOidcDiscoveryService
{
    Task<string> GetAuthorizationEndpointAsync(CancellationToken cancellationToken = default);

    Task<string> GetTokenEndpointAsync(CancellationToken cancellationToken = default);

    Task<string> GetUserInfoEndpointAsync(CancellationToken cancellationToken = default);

    Task<string> GetEndSessionEndpointAsync(CancellationToken cancellationToken = default);

    Task<string> GetJwksUriAsync(CancellationToken cancellationToken = default);
}
