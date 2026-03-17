using Microsoft.Extensions.Configuration;
using Microsoft.Playwright;
using Tests.E2E.Config;

namespace Tests.E2E.Fixtures
{
    /// <summary>
    /// Shared xUnit fixture that owns the Playwright browser process for a test collection.
    /// Each test class gets a fresh BrowserContext (isolated cookies/storage).
    /// </summary>
    public class PlaywrightFixture : IAsyncLifetime
    {
        public IPlaywright Playwright { get; private set; } = null!;
        public IBrowser Browser { get; private set; } = null!;
        public TestSettings Settings { get; private set; } = null!;

        public async Task InitializeAsync()
        {
            Settings = LoadSettings();

            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();

            Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = Settings.Headless,
                SlowMo = Settings.SlowMo,
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
                BaseURL = Settings.BaseUrl,
                ViewportSize = new ViewportSize { Width = 1280, Height = 720 },
            });

        private static TestSettings LoadSettings()
        {
            var config = new ConfigurationBuilder()
                .AddJsonFile("testsettings.json", optional: false)
                // testsettings.local.json is gitignored — use it to override
                // URLs or credentials without committing sensitive values
                .AddJsonFile("testsettings.local.json", optional: true)
                .AddEnvironmentVariables("E2E_")
                .Build();

            var settings = new TestSettings();
            config.Bind(settings);
            return settings;
        }
    }
}
