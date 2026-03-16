using Microsoft.Extensions.DependencyInjection;

namespace Musicratic.Voting.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddVotingApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
