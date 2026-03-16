extern alias HostApp;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Musicratic.Auth.Infrastructure.Persistence;
using Musicratic.Hub.Infrastructure.Persistence;
using Musicratic.Playback.Infrastructure.Persistence;
using Musicratic.TestUtilities.Auth;

namespace Musicratic.Integration.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<HostApp::Program>
{
    private readonly SqliteConnection _authConnection;
    private readonly SqliteConnection _hubConnection;
    private readonly SqliteConnection _playbackConnection;

    public CustomWebApplicationFactory()
    {
        _authConnection = new SqliteConnection("DataSource=:memory:");
        _authConnection.Open();

        _hubConnection = new SqliteConnection("DataSource=:memory:");
        _hubConnection.Open();

        _playbackConnection = new SqliteConnection("DataSource=:memory:");
        _playbackConnection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            RemoveDaprClient(services);
            ReplaceDbContext<AuthDbContext>(services, _authConnection);
            ReplaceDbContext<HubDbContext>(services, _hubConnection);
            ReplaceDbContext<PlaybackDbContext>(services, _playbackConnection);
            services.AddTestAuthentication();
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

        return host;
    }

    private static void RemoveDaprClient(IServiceCollection services)
    {
        var daprDescriptor = services.FirstOrDefault(d =>
            d.ServiceType.FullName?.Contains("DaprClient", StringComparison.Ordinal) == true);

        if (daprDescriptor is not null)
            services.Remove(daprDescriptor);
    }

    private static void ReplaceDbContext<TContext>(
        IServiceCollection services,
        SqliteConnection connection)
        where TContext : DbContext
    {
        var descriptor = services.SingleOrDefault(
            d => d.ServiceType == typeof(DbContextOptions<TContext>));

        if (descriptor is not null)
            services.Remove(descriptor);

        services.AddDbContext<TContext>((sp, options) =>
            options.UseSqlite(connection));
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _authConnection.Dispose();
            _hubConnection.Dispose();
            _playbackConnection.Dispose();
        }
    }
}
