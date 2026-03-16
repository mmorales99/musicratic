using Microsoft.Extensions.DependencyInjection;
using Musicratic.Voting.Application.Services;

namespace Musicratic.Voting.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddVotingApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        // VOTE-006: Voting window timer (singleton — in-memory state)
        services.AddSingleton<IVotingWindowService, VotingWindowService>();

        // VOTE-007 + VOTE-011: Skip rule engine
        services.AddSingleton<ISkipRuleEngine, SkipRuleEngine>();

        // VOTE-010: Anti-abuse rate limiter (singleton — in-memory counters)
        services.AddSingleton<IVoteRateLimiter, VoteRateLimiter>();

        return services;
    }
}
