using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Persistence;

namespace Tests.API.Fixtures;

/// <summary>
/// Spins up the real API in-process, replacing SQLite with an in-memory database
/// and injecting a test JWT token key so authentication works without any config file.
/// </summary>
public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Provide all config values the API expects so no appsettings.json is needed
        // HS512 requires >= 512 bits (64 bytes) — use a long enough test key
        builder.UseSetting("TokenKey", "super-secret-test-key-for-hs512-that-is-at-least-sixty-four-bytes-long!");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "DataSource=:memory:");

        // Capture the DB name once so all requests in a test run share the same data
        var dbName = "TestDb_" + Guid.NewGuid();

        builder.ConfigureServices(services =>
        {
            // Remove the real DataContext registration
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<DataContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            // Add an in-memory DataContext shared across all requests in this factory
            services.AddDbContext<DataContext>(options =>
                options.UseInMemoryDatabase(dbName));
        });
    }
}
