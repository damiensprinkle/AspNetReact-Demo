using System.Net.Http.Headers;
using Tests.Client;

namespace Tests.API.Fixtures;

/// <summary>
/// Base class for all API integration tests.
/// InitializeAsync runs before each test, building both an anonymous and an
/// authenticated typed client so individual tests don't need to handle auth setup.
/// </summary>
[Collection("Api")]
public abstract class ApiTestBase : IAsyncLifetime
{
    protected readonly ApiFactory Factory;

    // Shared test user credentials
    protected const string TestEmail    = "test@test.com";
    protected const string TestPassword = "Pa$$w0rd";
    protected const string TestUsername = "testuser";
    protected const string TestDisplay  = "Test User";

    /// <summary>Client with no Authorization header.</summary>
    protected IClient Anon { get; private set; } = null!;

    /// <summary>Client pre-configured with a valid Bearer token for the test user.</summary>
    protected IClient Auth { get; private set; } = null!;

    protected ApiTestBase(ApiFactory factory)
    {
        Factory = factory;
    }

    public async Task InitializeAsync()
    {
        Anon = CreateClient();

        // Register the test user (idempotent — ignore errors if already exists)
        try
        {
            await Anon.RegisterAsync(new RegisterDto
            {
                Email       = TestEmail,
                Password    = TestPassword,
                Username    = TestUsername,
                DisplayName = TestDisplay,
            });
        }
        catch { /* already exists */ }

        var user = await Anon.LoginAsync(new LoginDto
        {
            Email    = TestEmail,
            Password = TestPassword,
        });

        var http = Factory.CreateClient();
        http.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", user.Token);

        Auth = ApiClientFactory.Wrap("http://localhost/", http);
    }

    public Task DisposeAsync() => Task.CompletedTask;

    /// <summary>Creates a fresh anonymous typed client wrapping the in-process factory.</summary>
    protected IClient CreateClient() =>
        ApiClientFactory.Wrap("http://localhost/", Factory.CreateClient());
}
