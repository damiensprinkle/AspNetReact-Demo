using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace Tests.E2E.Pages
{
    /// <summary>The create/edit activity form at /activities/create or /activities/:id/edit.</summary>
    public class ActivityFormPage(IPage page)
    {
        public ILocator TitleInput       => page.GetByPlaceholder("Title");
        public ILocator DescriptionInput => page.GetByPlaceholder("Description");
        public ILocator CategoryInput    => page.GetByPlaceholder("Category");
        public ILocator DateInput        => page.Locator("input[name='date']");
        public ILocator CityInput        => page.GetByPlaceholder("City");
        public ILocator VenueInput       => page.GetByPlaceholder("Venue");
        public ILocator SubmitButton     => page.Locator("[data-testid='submit-button']");
        public ILocator CancelButton     => page.Locator("[data-testid='cancel-button']");

        public async Task NavigateToCreateAsync() =>
            await page.GotoAsync("/activities/create");

        public async Task FillFormAsync(string title, string description, string category,
            string date, string city, string venue)
        {
            await TitleInput.FillAsync(title);
            await DescriptionInput.FillAsync(description);
            await CategoryInput.FillAsync(category);
            await DateInput.FillAsync(date);
            await CityInput.FillAsync(city);
            await VenueInput.FillAsync(venue);
        }

        /// <summary>Submits the form and returns the activity detail page.</summary>
        public async Task<ActivityDetailsPage> SubmitAsync()
        {
            await SubmitButton.ClickAsync();
            await page.WaitForURLAsync(new Regex(@"/activities/[0-9a-f-]{36}$"));
            return new ActivityDetailsPage(page);
        }

        /// <summary>Cancels the form and returns to the activities list.</summary>
        public async Task<ActivitiesPage> CancelAsync()
        {
            await CancelButton.ClickAsync();
            await page.WaitForURLAsync("**/activities");
            return new ActivitiesPage(page);
        }
    }
}
