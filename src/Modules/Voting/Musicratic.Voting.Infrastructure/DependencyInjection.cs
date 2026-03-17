using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Musicratic.Shared.Application;
using Musicratic.Voting.Application.Services;
using Musicratic.Voting.Domain.Repositories;
using Musicratic.Voting.Infrastructure.Persistence;
using Musicratic.Voting.Infrastructure.Services;

namespace Musicratic.Voting.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddVotingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<VotingDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("VotingDb")));

        services.AddScoped<IVoteRepository, VoteRepository>();
        services.AddScoped<ICollectiveVoteSessionRepository, CollectiveVoteSessionRepository>();
        services.AddScoped<IUnitOfWork, VotingUnitOfWork>();

        // VOTE-009: WebSocket connection manager + broadcast service
        services.AddSingleton<VoteConnectionManager>();
        services.AddSingleton<IVoteConnectionManager>(sp =>
            sp.GetRequiredService<VoteConnectionManager>());
        services.AddScoped<IVoteTallyBroadcastService, VoteTallyBroadcastService>();

        // VOTE-013: Dapr event publisher
        services.AddScoped<IVoteEventPublisher, VoteEventPublisher>();

        // VOTE-008: Owner priority vote service
        services.AddScoped<IOwnerVoteService, OwnerVoteService>();

        // VOTE-013: MediatR handlers for Dapr event subscriptions
        services.AddMediatR(cfg =>
            cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly));

        return services;
    }
}
