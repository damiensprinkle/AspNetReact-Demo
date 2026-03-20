using Tests.E2E.Fixtures;
using Tests.E2E.Pages;
using Xunit.Abstractions;

namespace Tests.E2E.Tests
{
    [Collection("Playwright.1")]
    public class ActivityListTests : PageTestBase
    {
        public ActivityListTests(PlaywrightFixturePool1 fixture, ITestOutputHelper output)
            : base(fixture, output) { }

        [Fact]
        public async Task ActivitiesPage_LoadsAndDisplaysActivities()
        {
            var activitiesPage = new ActivitiesPage(Page);
            await activitiesPage.NavigateAsync();
            await activitiesPage.WaitForActivitiesAsync();

            var count = await activitiesPage.ActivityCountAsync();
            Assert.True(count > 0, "Expected at least one seeded activity to be visible");
        }

        [Fact]
        public async Task ActivitiesPage_ClickView_NavigatesToDetailPage()
        {
            var activitiesPage = new ActivitiesPage(Page);
            await activitiesPage.NavigateAsync();
            await activitiesPage.WaitForActivitiesAsync();

            var title = await activitiesPage.FirstActivityTitle.InnerTextAsync();

            var detailsPage = await activitiesPage.ClickViewAsync(title);

            Assert.Equal(title, await detailsPage.GetTitleAsync());
        }

        [Fact]
        public async Task ActivitiesPage_RootRedirectsToActivities()
        {
            await Page.GotoAsync("/");
            await Page.WaitForURLAsync("**/activities");
            Assert.Contains("/activities", Page.Url);
        }
    }
}
