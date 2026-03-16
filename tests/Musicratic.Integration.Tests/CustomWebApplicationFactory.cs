extern alias HostApp;

using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Musicratic.Integration.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<HostApp::Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // Remove Dapr client to avoid requiring a running sidecar in tests
            var daprDescriptor = services.FirstOrDefault(d =>
                d.ServiceType.FullName?.Contains("DaprClient", StringComparison.Ordinal) == true);

            if (daprDescriptor is not null)
                services.Remove(daprDescriptor);
        });
    }
}
