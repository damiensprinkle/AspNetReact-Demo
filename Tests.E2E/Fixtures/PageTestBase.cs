using System.Reflection;
using Microsoft.Playwright;
using Tests.Client;
using Tests.Client.Builders;
using Tests.Client.Mothers;
using Tests.E2E.Config;
using Xunit.Abstractions;

namespace Tests.E2E.Fixtures
{
    /// <summary>
    /// Base class for all Playwright tests.
    /// Gives each test its own IBrowserContext and IPage, then tears them down.
    ///
    /// If a test method (or its class, as a fallback) is decorated with [UseAccount],
    /// the matching account from the fixture's pool is automatically logged in before
    /// the test runs.  Because each parallel collection has its own pool, concurrent
    /// tests in different collections never share credentials.
    /// </summary>
    public abstract class PageTestBase : IAsyncLifetime
    {
        protected readonly PlaywrightFixture Fixture;
        protected IBrowserContext Context { get; private set; } = null!;
        protected IPage Page { get; private set; } = null!;
        protected TestSettings Settings => Fixture.Settings;

        private readonly ITestOutputHelper _output;

        // Stored after login so mothers can reuse it (browser session token)
        private string? _authToken;

        // Separate client for GetApiAsync() — authenticated independently of the browser session
        private IClient? _apiClient;
        private TestUserSettings? _resolvedAccount;

        /// <summary>
        /// API-backed activity mother. Available after login; use in tests to obtain
        /// cached <see cref="ActivityDto"/> instances created via the real API.
        /// </summary>
        protected ActivityMother Activities { get; private set; } = null!;

        /// <summary>
        /// API-backed user mother. Available after login; use in tests to obtain
        /// cached <see cref="UserDto"/> instances registered via the real API.
        /// </summary>
        protected UserMother Users { get; private set; } = null!;

        protected PageTestBase(PlaywrightFixture fixture, ITestOutputHelper output)
        {
            Fixture = fixture;
            _output = output;
        }

        public virtual async Task InitializeAsync()
        {
            Context = await Fixture.NewContextAsync();
            Page    = await Context.NewPageAsync();

            var account = ResolveAccount();
            if (account is not null)
            {
                _resolvedAccount = Fixture.GetAccount(account.Value);
                await LoginAsync(_resolvedAccount);
            }
        }

        public virtual async Task DisposeAsync()
        {
            await Context.CloseAsync();
        }

        // ── Auth helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns the credentials for the given account from the current pool.
        /// </summary>
        protected TestUserSettings GetAccount(AutomationAccount account)
        {
            return Fixture.GetAccount(account);
        }

        /// <summary>
        /// Logs in as the SysAdmin account from the current pool.
        /// Call explicitly in tests that are not decorated with [UseAccount].
        /// </summary>
        protected Task LoginAsTestUserAsync()
        {
            return LoginAsync(Fixture.GetAccount(AutomationAccount.SysAdmin));
        }

        /// <summary>
        /// Registers the given account via the API (idempotent), logs in, then injects
        /// the JWT into localStorage so the app treats the session as authenticated.
        /// Also stores the token for use by SeedActivityAsync.
        /// </summary>
        private async Task LoginAsync(TestUserSettings account)
        {
            var client = ApiClientFactory.CreateAnonymous(Settings.ApiUrl);

            // Register — ignore errors (user may already exist)
            try
            {
                await client.RegisterAsync(new RegisterDto
                {
                    Email       = account.Email,
                    Password    = account.Password,
                    DisplayName = account.DisplayName,
                    Username    = account.Username,
                });
            }
            catch { /* already exists */ }

            var user = await client.LoginAsync(new LoginDto
            {
                Email    = account.Email,
                Password = account.Password,
            });

            _authToken = user.Token;

            var sessionClient = ApiClientFactory.CreateAuthenticated(Settings.ApiUrl, _authToken);
            Activities = new ActivityMother(sessionClient);
            Users      = new UserMother(sessionClient);

            // Navigate first so the page has an origin for localStorage
            await Page.GotoAsync("/");

            // Pass the token as an argument — avoids any risk of JS injection
            await Page.EvaluateAsync("token => localStorage.setItem('jwt', token)", _authToken);

            await Page.ReloadAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        }

        // ── API / Seed helpers ────────────────────────────────────────────────────

        /// <summary>
        /// Returns an authenticated API client for the current test account.
        /// Authenticates independently of the browser session — safe to use alongside
        /// an active Playwright page for the same user (JWT is stateless; both tokens are valid).
        /// The client is created once per test and reused on subsequent calls.
        /// Requires that a login has already occurred (either via [UseAccount] or LoginAsTestUserAsync).
        /// </summary>
        protected async Task<IClient> GetApiAsync()
        {
            if (_resolvedAccount is null)
                throw new InvalidOperationException(
                    "No account resolved. Decorate the test with [UseAccount] or call LoginAsTestUserAsync first.");

            if (_apiClient is not null)
                return _apiClient;

            var anonClient = ApiClientFactory.CreateAnonymous(Settings.ApiUrl);
            var user       = await anonClient.LoginAsync(new LoginDto
            {
                Email    = _resolvedAccount.Email,
                Password = _resolvedAccount.Password,
            });

            _apiClient = ApiClientFactory.CreateAuthenticated(Settings.ApiUrl, user.Token);
            return _apiClient;
        }

        /// <summary>
        /// Creates an activity directly via the API (bypassing the UI) and returns its ID.
        /// Pass a custom <see cref="ActivityFormDto"/> built with <see cref="ActivityFormDtoBuilder"/>
        /// for one-off test data; for shared cached instances prefer <see cref="Activities"/>.
        /// Requires that a login has already occurred (either via [UseAccount] or LoginAsTestUserAsync).
        /// </summary>
        protected async Task<string> SeedActivityAsync(ActivityFormDto? dto = null)
        {
            if (Activities is null)
                throw new InvalidOperationException(
                    "No auth token available. Decorate the test with [UseAccount] or call LoginAsTestUserAsync first.");

            var activity = dto is not null
                ? await Activities.CreateOnceAsync(dto)
                : await Activities.DefaultAsync();

            return activity.Id.ToString();
        }


        // ── Account resolution ────────────────────────────────────────────────────

        /// <summary>
        /// Resolves the <see cref="AutomationAccount"/> for the current test.
        /// Method-level [UseAccount] takes precedence over class-level.
        /// Returns null if neither is present (no auto-login).
        /// </summary>
        private AutomationAccount? ResolveAccount()
        {
            // xUnit v2: TestOutputHelper holds a private ITest field that exposes the method name,
            // letting us reflect on [UseAccount] before the test body runs.
            // Search by field type rather than by name — the field is "test" in some runner
            // builds and "_test" in others, so matching on name is fragile across environments.
            var testField = _output.GetType()
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .FirstOrDefault(f => typeof(ITest).IsAssignableFrom(f.FieldType));

            if (testField?.GetValue(_output) is not ITest test)
                return null;

            var methodName = test.TestCase.TestMethod.Method.Name;
            var type       = GetType();
            var method     = type.GetMethod(methodName, BindingFlags.Public | BindingFlags.Instance);

            return (method?.GetCustomAttribute<UseAccountAttribute>()
                    ?? type.GetCustomAttribute<UseAccountAttribute>())?.Account;
        }
    }
}
