using Tests.E2E.Fixtures;
using Tests.E2E.Pages;
using Xunit.Abstractions;

namespace Tests.E2E.Tests
{
    [Collection("Playwright.2")]
    public class NavigationTests : PageTestBase
    {
        public NavigationTests(PlaywrightFixturePool2 fixture, ITestOutputHelper output)
            : base(fixture, output) { }

        [Fact]
        public async Task DetailPage_CancelButton_ReturnsToActivityList()
        {
            var activitiesPage = new ActivitiesPage(Page);
            await activitiesPage.NavigateAsync();
            await activitiesPage.WaitForActivitiesAsync();

            var detailsPage = await activitiesPage.ClickFirstViewAsync();
            await detailsPage.ClickCancelAsync();

            Assert.Matches(@".*/activities$", Page.Url);
        }

        [Fact]
        public async Task NavBar_ActivitiesLink_NavigatesToDashboard()
        {
            await Page.GotoAsync("/");
            var navBar = new NavBarPage(Page);
            await navBar.ClickActivitiesAsync();

            await Page.WaitForURLAsync("**/activities");
            Assert.Contains("/activities", Page.Url);
        }

        [Fact]
        public async Task DirectUrl_ActivityDetail_LoadsCorrectActivity()
        {
            var activitiesPage = new ActivitiesPage(Page);
            await activitiesPage.NavigateAsync();
            await activitiesPage.WaitForActivitiesAsync();

            // Click the first View button and capture title + URL from the detail page
            // (avoids any ordering dependency between list title and detail page)
            var detailsPage = await activitiesPage.ClickFirstViewAsync();
            var detailUrl   = Page.Url;
            var title       = await detailsPage.GetTitleAsync();

            // Navigate away then deep-link back directly
            await Page.GotoAsync("/activities");
            await Page.GotoAsync(detailUrl);
            await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);

            Assert.Equal(title, await detailsPage.GetTitleAsync());
        }
    }
}
