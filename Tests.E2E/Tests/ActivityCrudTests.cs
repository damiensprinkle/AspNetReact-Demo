using Tests.E2E.Fixtures;
using Tests.E2E.Pages;

namespace Tests.E2E.Tests
{
    [Collection("Playwright")]
    public class ActivityCrudTests : PageTestBase
    {
        public ActivityCrudTests(PlaywrightFixture fixture) : base(fixture) { }

        public override async Task InitializeAsync()
        {
            await base.InitializeAsync();
            await LoginAsTestUserAsync();
        }

        [Fact]
        public async Task CreateActivity_WithValidData_AppearsOnDetailPage()
        {
            var title    = $"E2E Activity {Guid.NewGuid():N}";
            var formPage = new ActivityFormPage(Page);
            await formPage.NavigateToCreateAsync();

            await formPage.FillFormAsync(
                title:       title,
                description: "Created by Playwright",
                category:    "culture",
                date:        "2030-06-15",
                city:        "London",
                venue:       "Test Venue");

            var detailsPage = await formPage.SubmitAsync();

            Assert.Equal(title, await detailsPage.GetTitleAsync());
        }

        [Fact]
        public async Task CreateActivity_ThenNavigateToList_ShowsNewActivity()
        {
            var title    = $"List Check {Guid.NewGuid():N}";
            var formPage = new ActivityFormPage(Page);
            await formPage.NavigateToCreateAsync();

            await formPage.FillFormAsync(
                title:       title,
                description: "Verify in list",
                category:    "music",
                date:        "2030-07-20",
                city:        "Manchester",
                venue:       "Arena");

            await formPage.SubmitAsync();

            var activitiesPage = new ActivitiesPage(Page);
            await activitiesPage.NavigateAsync();
            await activitiesPage.WaitForActivitiesAsync();

            await activitiesPage.ItemWithTitle(title).WaitForAsync();
            Assert.True(await activitiesPage.ItemWithTitle(title).IsVisibleAsync());
        }

        [Fact]
        public async Task EditActivity_UpdatesTitle_ReflectedInDetails()
        {
            // Seed via API — the create flow is not what this test is testing
            var activityId = await SeedActivityAsync($"Edit Me {Guid.NewGuid():N}");

            await Page.GotoAsync($"/activities/{activityId}");
            var detailsPage = new ActivityDetailsPage(Page);

            var editFormPage = await detailsPage.ClickEditAsync();

            var updatedTitle = $"Edited {Guid.NewGuid():N}";
            await editFormPage.TitleInput.ClearAsync();
            await editFormPage.TitleInput.FillAsync(updatedTitle);

            var updatedDetails = await editFormPage.SubmitAsync();

            Assert.Equal(updatedTitle, await updatedDetails.GetTitleAsync());
        }

        [Fact]
        public async Task DeleteActivity_RemovesItFromList()
        {
            // Seed via API — the create flow is not what this test is testing
            var title = $"Delete Me {Guid.NewGuid():N}";
            await SeedActivityAsync(title);

            var activitiesPage = new ActivitiesPage(Page);
            await activitiesPage.NavigateAsync();
            await activitiesPage.WaitForActivitiesAsync();

            var item = activitiesPage.ItemWithTitle(title);
            await item.WaitForAsync();
            await activitiesPage.ClickDeleteAsync(title);

            await item.WaitForAsync(new() { State = Microsoft.Playwright.WaitForSelectorState.Hidden });
            Assert.False(await item.IsVisibleAsync());
        }

        [Fact]
        public async Task CreateActivity_CancelButton_ReturnsToList()
        {
            var formPage = new ActivityFormPage(Page);
            await formPage.NavigateToCreateAsync();

            var activitiesPage = await formPage.CancelAsync();

            Assert.Contains("/activities", Page.Url);
            Assert.DoesNotContain("/create", Page.Url);
            _ = activitiesPage; // returned page object available for further assertions
        }
    }
}
