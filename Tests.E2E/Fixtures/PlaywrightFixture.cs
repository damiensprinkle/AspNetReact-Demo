using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Tests.E2E.Config;

namespace Tests.E2E.Fixtures
{
    /// <summary>
    /// Shared xUnit fixture that owns the Playwright browser process for a test collection.
    /// Each test class gets a fresh BrowserContext (isolated cookies/storage).
    ///
    /// Each concrete subclass declares a PoolName that maps to a set of credentials in
    /// testsettings.json, ensuring parallel collections never share accounts.
    /// </summary>
    public abstract class PlaywrightFixture : IAsyncLifetime
    {
        public IPlaywright Playwright { get; private set; } = null!;
        public IBrowser Browser { get; private set; } = null!;
        public TestSettings Settings { get; private set; } = null!;

        /// <summary>
        /// The pool key used to look up this collection's accounts in testsettings.json.
        /// Override in a subclass to assign a different pool.
        /// </summary>
        protected abstract string PoolName { get; }

        public async Task InitializeAsync()
        {
            Settings = LoadSettings();

            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = Settings.Headless,
                SlowMo   = Settings.SlowMo,
            });
        }

        public async Task DisposeAsync()
        {
            await Browser.CloseAsync();
            Playwright.Dispose();
        }

        /// <summary>Creates a fresh isolated browser context for a single test.</summary>
        public async Task<IBrowserContext> NewContextAsync() =>
            await Browser.NewContextAsync(new BrowserNewContextOptions
            {
                BaseURL      = Settings.BaseUrl,
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
            });

        /// <summary>
        /// Returns the credentials for the given account role from this collection's pool.
        /// </summary>
        public TestUserSettings GetAccount(AutomationAccount account) =>
            Settings.GetAccount(PoolName, account);

        private static TestSettings LoadSettings()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json", optional: false)
                // testsettings.local.json is gitignored — override URLs or credentials without committing
                .AddJsonFile("testsettings.local.json", optional: true)
                .AddEnvironmentVariables("E2E_")
                .Build();

            var settings = new TestSettings();
            config.Bind(settings);
            return settings;
        }
    }

    /// <summary>Uses the Pool1 account set. Assigned to the "Playwright.1" collection.</summary>
    public class PlaywrightFixturePool1 : PlaywrightFixture
    {
        protected override string PoolName => "Pool1";
    }

    /// <summary>Uses the Pool2 account set. Assigned to the "Playwright.2" collection.</summary>
    public class PlaywrightFixturePool2 : PlaywrightFixture
    {
        protected override string PoolName => "Pool2";
    }
}
