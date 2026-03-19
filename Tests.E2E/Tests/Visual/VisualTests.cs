using Microsoft.Playwright;
using Tests.E2E.Config;
using Tests.E2E.Fixtures;
using Tests.E2E.Visual;
using Xunit.Abstractions;

namespace Tests.E2E.Tests.Visual
{
    /// <summary>
    /// Visual regression tests. Each test captures a full-page screenshot and compares it
    /// against the committed baseline in <c>Tests/Visual/Baselines/</c>.
    ///
    /// First run: baselines are created automatically and tests pass with "New Baseline" status.
    /// Subsequent runs: pixel differences are measured; the test fails if they exceed the threshold.
    ///
    /// To update baselines after an intentional UI change:
    ///   set UPDATE_VISUAL_BASELINES=true
    ///   dotnet test Tests.E2E --filter "FullyQualifiedName~VisualTests"
    /// Then copy the updated PNGs from bin/.../Baselines/ back into Tests/Visual/Baselines/ and commit.
    ///
    /// The HTML report is written to bin/.../VisualResults/report.html after each run.
    /// </summary>
    [Collection("Playwright.1")]
    public class VisualTests : PageTestBase, IClassFixture<VisualTestContext>
    {
        private readonly VisualTestContext _visual;

        public VisualTests(PlaywrightFixture fixture, ITestOutputHelper output, VisualTestContext visual)
            : base(fixture, output)
        {
            _visual = visual;
        }

        [Fact]
        [UseAccount(AutomationAccount.SysAdmin)]
        public async Task LoginPage_MatchesBaseline()
        {
            await Page.GotoAsync("/login");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var screenshot = await Page.ScreenshotAsync(new() { FullPage = true });
            var result = _visual.Comparer.Compare("login-page", screenshot);
            _visual.AddResult(result);

            Assert.True(result.Status != VisualTestStatus.Failed,
                $"login-page: {result.DiffFraction:P3} of pixels differ (threshold {_visual.Comparer.Threshold:P3}).");
        }

        [Fact]
        [UseAccount(AutomationAccount.SysAdmin)]
        public async Task RegisterPage_MatchesBaseline()
        {
            await Page.GotoAsync("/register");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var screenshot = await Page.ScreenshotAsync(new() { FullPage = true });
            var result = _visual.Comparer.Compare("register-page", screenshot);
            _visual.AddResult(result);

            Assert.True(result.Status != VisualTestStatus.Failed,
                $"register-page: {result.DiffFraction:P3} of pixels differ (threshold {_visual.Comparer.Threshold:P3}).");
        }

        [Fact]
        [UseAccount(AutomationAccount.SysAdmin)]
        public async Task ActivitiesPage_MatchesBaseline()
        {
            await Page.GotoAsync("/activities");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var screenshot = await Page.ScreenshotAsync(new() { FullPage = true });
            var result = _visual.Comparer.Compare("activities-page", screenshot);
            _visual.AddResult(result);

            Assert.True(result.Status != VisualTestStatus.Failed,
                $"activities-page: {result.DiffFraction:P3} of pixels differ (threshold {_visual.Comparer.Threshold:P3}).");
        }

        [Fact]
        [UseAccount(AutomationAccount.SysAdmin)]
        public async Task ActivityDetailPage_MatchesBaseline()
        {
            await Page.GotoAsync("/activities");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Click the first activity to land on a detail page
            var viewButton = Page.Locator("[data-testid='view-button']").First;
            await viewButton.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var screenshot = await Page.ScreenshotAsync(new() { FullPage = true });
            var result = _visual.Comparer.Compare("activity-detail-page", screenshot);
            _visual.AddResult(result);

            Assert.True(result.Status != VisualTestStatus.Failed,
                $"activity-detail-page: {result.DiffFraction:P3} of pixels differ (threshold {_visual.Comparer.Threshold:P3}).");
        }

        [Fact]
        [UseAccount(AutomationAccount.SysAdmin)]
        public async Task CreateActivityPage_MatchesBaseline()
        {
            await Page.GotoAsync("/activities/create");
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            var screenshot = await Page.ScreenshotAsync(new() { FullPage = true });
            var result = _visual.Comparer.Compare("create-activity-page", screenshot);
            _visual.AddResult(result);

            Assert.True(result.Status != VisualTestStatus.Failed,
                $"create-activity-page: {result.DiffFraction:P3} of pixels differ (threshold {_visual.Comparer.Threshold:P3}).");
        }
    }
}
