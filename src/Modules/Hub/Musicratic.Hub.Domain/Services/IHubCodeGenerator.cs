namespace Musicratic.Hub.Domain.Services;

public interface IHubCodeGenerator
{
    Task<string> Generate(string hubName, CancellationToken cancellationToken = default);
}
