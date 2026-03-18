extern alias HostApp;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Musicratic.Analytics.Infrastructure.Persistence;
using Musicratic.Auth.Infrastructure.Persistence;
using Musicratic.Economy.Infrastructure.Persistence;
using Musicratic.Hub.Infrastructure.Persistence;
using Musicratic.Notification.Infrastructure.Persistence;
using Musicratic.Playback.Infrastructure.Persistence;
using Musicratic.Social.Infrastructure.Persistence;
using Musicratic.TestUtilities.Auth;
using Musicratic.Voting.Application.Services;
using Musicratic.Voting.Domain.Enums;
using Musicratic.Voting.Infrastructure.Persistence;

namespace Musicratic.Integration.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<HostApp::Program>
{
    private readonly List<SqliteConnection> _connections = [];
    private static int _dbCounter;

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            RemoveDaprClient(services);
            RemoveHostedServices(services);

            ReplaceDbContext<AuthDbContext>(services);
            ReplaceDbContext<HubDbContext>(services);
            ReplaceDbContext<PlaybackDbContext>(services);
            ReplaceDbContext<VotingDbContext>(services);
            ReplaceDbContext<EconomyDbContext>(services);
            ReplaceDbContext<AnalyticsDbContext>(services);
            ReplaceDbContext<SocialDbContext>(services);
            ReplaceDbContext<NotificationDbContext>(services);

            services.AddTestAuthentication();

            // Replace Dapr-dependent VoteEventPublisher with no-op for tests
            ReplaceService<IVoteEventPublisher, NoOpVoteEventPublisher>(services);
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var sp = scope.ServiceProvider;
        sp.GetRequiredService<AuthDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<HubDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<PlaybackDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<VotingDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<EconomyDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<AnalyticsDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<SocialDbContext>().Database.EnsureCreated();
        sp.GetRequiredService<NotificationDbContext>().Database.EnsureCreated();

        return host;
    }

    private static void RemoveDaprClient(IServiceCollection services)
    {
        var daprDescriptors = services
            .Where(d => d.ServiceType.FullName?.Contains("DaprClient", StringComparison.Ordinal) == true)
            .ToList();

        foreach (var d in daprDescriptors)
            services.Remove(d);
    }

    private static void RemoveHostedServices(IServiceCollection services)
    {
        var hostedServices = services
            .Where(d => d.ServiceType == typeof(IHostedService))
            .ToList();

        foreach (var d in hostedServices)
            services.Remove(d);
    }

    private void ReplaceDbContext<TContext>(IServiceCollection services)
        where TContext : DbContext
    {
        // Remove ALL registrations related to TContext more aggressively
        var contextTypeName = typeof(TContext).Name;
        var descriptorsToRemove = services
            .Where(d =>
                d.ServiceType == typeof(TContext) ||
                d.ServiceType == typeof(DbContextOptions<TContext>) ||
                (d.ServiceType.IsGenericType &&
                 d.ServiceType.GenericTypeArguments.Any(a => a == typeof(TContext))))
            .ToList();

        foreach (var d in descriptorsToRemove)
            services.Remove(d);

        // Use shared named in-memory database so all scopes access the same data
        var dbName = $"TestDb_{typeof(TContext).Name}_{Interlocked.Increment(ref _dbCounter)}";
        var connectionString = $"DataSource={dbName};Mode=Memory;Cache=Shared";

        var keepAliveConnection = new SqliteConnection(connectionString);
        keepAliveConnection.Open();
        _connections.Add(keepAliveConnection);

        services.AddDbContext<TContext>((sp, options) =>
            options.UseSqlite(connectionString));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            foreach (var conn in _connections)
                conn.Dispose();
        }
    }

    private static void ReplaceService<TService, TImplementation>(IServiceCollection services)
        where TService : class
        where TImplementation : class, TService
    {
        var descriptors = services
            .Where(d => d.ServiceType == typeof(TService))
            .ToList();

        foreach (var d in descriptors)
            services.Remove(d);

        services.AddScoped<TService, TImplementation>();
    }
}

internal sealed class NoOpVoteEventPublisher : IVoteEventPublisher
{
    public Task PublishVoteCastAsync(
        Guid tenantId, Guid queueEntryId, Guid userId, VoteValue value,
        CancellationToken cancellationToken = default) => Task.CompletedTask;

    public Task PublishSkipTriggeredAsync(
        Guid tenantId, Guid queueEntryId, string reason, double downvotePercentage,
        CancellationToken cancellationToken = default) => Task.CompletedTask;
}
