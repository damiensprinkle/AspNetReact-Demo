using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace Tests.E2E.Pages
{
    /// <summary>The activity list dashboard at /activities.</summary>
    public class ActivitiesPage(IPage page)
    {
        public ILocator ActivityItems => page.Locator("[data-testid='activity-item']");
        public ILocator FirstActivityTitle => ActivityItems.First.Locator("[data-testid='activity-title']");

        public ILocator ItemWithTitle(string title) =>
            page.Locator("[data-testid='activity-item']", new() { HasText = title });

        public ILocator ViewButtonFor(string title) =>
            ItemWithTitle(title).Locator("[data-testid='view-button']");

        public ILocator DeleteButtonFor(string title) =>
            ItemWithTitle(title).Locator("[data-testid='delete-button']");

        public async Task NavigateAsync() =>
            await page.GotoAsync("/activities");

        public async Task WaitForActivitiesAsync() =>
            await page.WaitForSelectorAsync("[data-testid='activity-item']");

        public async Task<int> ActivityCountAsync() =>
            await ActivityItems.CountAsync();

        /// <summary>Clicks the View button for the named activity and returns the detail page.</summary>
        public async Task<ActivityDetailsPage> ClickViewAsync(string title)
        {
            await ViewButtonFor(title).ClickAsync();
            await page.WaitForURLAsync(new Regex(@"/activities/[0-9a-f-]{36}$"));
            return new ActivityDetailsPage(page);
        }

        /// <summary>Clicks the View button on the first activity without filtering by title.</summary>
        public async Task<ActivityDetailsPage> ClickFirstViewAsync()
        {
            await ActivityItems.First.Locator("[data-testid='view-button']").ClickAsync();
            await page.WaitForURLAsync(new Regex(@"/activities/[0-9a-f-]{36}$"));
            return new ActivityDetailsPage(page);
        }

        public async Task ClickDeleteAsync(string title) =>
            await DeleteButtonFor(title).ClickAsync();
    }
}
