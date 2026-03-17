using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace Tests.E2E.Pages
{
    /// <summary>The activity detail card at /activities/:id.</summary>
    public class ActivityDetailsPage(IPage page)
    {
        public ILocator Title => page.Locator("[data-testid='activity-title']");
        public ILocator Date => page.Locator("[data-testid='activity-date']");
        public ILocator Description => page.Locator("[data-testid='activity-description']");
        public ILocator EditButton => page.Locator("[data-testid='edit-button']");
        public ILocator CancelButton => page.Locator("[data-testid='cancel-button']");

        public async Task<string> GetTitleAsync() => await Title.InnerTextAsync();
        public async Task<string> GetDescriptionAsync() => await Description.InnerTextAsync();

        /// <summary>Clicks Edit and returns the form page for this activity.</summary>
        public async Task<ActivityFormPage> ClickEditAsync()
        {
            await EditButton.ClickAsync();
            await page.WaitForURLAsync(new Regex(@"/activities/[0-9a-f-]{36}/edit$"));
            return new ActivityFormPage(page);
        }

        /// <summary>Clicks Cancel and returns to the activities list.</summary>
        public async Task<ActivitiesPage> ClickCancelAsync()
        {
            await CancelButton.ClickAsync();
            await page.WaitForURLAsync("**/activities");
            return new ActivitiesPage(page);
        }
    }
}
