using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.Playwright;
using Tests.E2E.Config;

namespace Tests.E2E.Fixtures
{
    /// <summary>
    /// Base class for all Playwright tests.
    /// Gives each test its own IBrowserContext and IPage, then tears them down.
    /// </summary>
    public abstract class PageTestBase : IAsyncLifetime
    {
        protected readonly PlaywrightFixture Fixture;
        protected IBrowserContext Context { get; private set; } = null!;
        protected IPage Page { get; private set; } = null!;
        protected TestSettings Settings => Fixture.Settings;

        // Stored after LoginAsTestUserAsync so SeedActivityAsync can reuse it
        private string? _authToken;

        protected PageTestBase(PlaywrightFixture fixture)
        {
            Fixture = fixture;
        }

        public virtual async Task InitializeAsync()
        {
            Context = await Fixture.NewContextAsync();
            Page = await Context.NewPageAsync();
        }

        public virtual async Task DisposeAsync()
        {
            await Context.CloseAsync();
        }

        /// <summary>
        /// Registers the test user via the API (idempotent), logs in, then injects
        /// the JWT into localStorage so the app treats the session as authenticated.
        /// Also stores the token for use by SeedActivityAsync.
        /// </summary>
        protected async Task LoginAsTestUserAsync()
        {
            using var http = new HttpClient { BaseAddress = new Uri(Settings.ApiUrl) };

            // Register — ignore errors (user may already exist)
            await http.PostAsJsonAsync("/account/register", new
            {
                email       = Settings.TestUser.Email,
                password    = Settings.TestUser.Password,
                displayName = Settings.TestUser.DisplayName,
                username    = Settings.TestUser.Username,
            });

            var loginResponse = await http.PostAsJsonAsync("/account/login", new
            {
                email    = Settings.TestUser.Email,
                password = Settings.TestUser.Password,
            });

            loginResponse.EnsureSuccessStatusCode();

            var body = await loginResponse.Content.ReadFromJsonAsync<TokenResponse>();
            _authToken = body!.Token;

            // Navigate first so the page has an origin for localStorage
            await Page.GotoAsync("/");

            // Pass the token as an argument — avoids any risk of JS string injection
            await Page.EvaluateAsync("token => localStorage.setItem('jwt', token)", _authToken);

            await Page.ReloadAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        /// <summary>
        /// Creates an activity directly via the API (bypassing the UI) and returns its ID.
        /// Call after LoginAsTestUserAsync so the auth token is available.
        /// </summary>
        protected async Task<string> SeedActivityAsync(
            string title,
            string description = "Seeded by test",
            string category    = "culture",
            string city        = "London",
            string venue       = "Test Venue",
            string date        = "2030-01-01")
        {
            if (_authToken is null)
                throw new InvalidOperationException("Call LoginAsTestUserAsync before SeedActivityAsync.");

            using var http = new HttpClient { BaseAddress = new Uri(Settings.ApiUrl) };
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _authToken);

            var response = await http.PostAsJsonAsync("/activities", new
            {
                title,
                description,
                category,
                city,
                venue,
                date = date + "T00:00:00Z",
            });

            response.EnsureSuccessStatusCode();

            var activity = await response.Content.ReadFromJsonAsync<ActivityResponse>();
            return activity!.Id;
        }

        private record TokenResponse(string Token, string DisplayName, string Username);
        private record ActivityResponse(string Id, string Title);
    }
}
