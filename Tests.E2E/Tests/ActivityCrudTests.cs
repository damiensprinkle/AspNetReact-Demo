using Tests.Client.Builders;
using Tests.E2E.Config;
using Tests.E2E.Fixtures;
using Tests.E2E.Pages;
using Xunit.Abstractions;

namespace Tests.E2E.Tests
{
    [Collection("Playwright.1")]
    public class ActivityCrudTests : PageTestBase
    {
        public ActivityCrudTests(PlaywrightFixture fixture, ITestOutputHelper output)
            : base(fixture, output) { }

        [Fact]
        [UseAccount(AutomationAccount.SysAdmin)]
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
        [UseAccount(AutomationAccount.SysAdmin)]
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
        [UseAccount(AutomationAccount.SysAdmin)]
        public async Task EditActivity_UpdatesTitle_ReflectedInDetails()
        {
            var activityId = await SeedActivityAsync(
                new ActivityFormDtoBuilder().Set(x => x.Title, $"Edit Me {Guid.NewGuid():N}").Build());

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
        [UseAccount(AutomationAccount.SysAdmin)]
        public async Task DeleteActivity_RemovesItFromList()
        {
            var title = $"Delete Me {Guid.NewGuid():N}";
            await SeedActivityAsync(
                new ActivityFormDtoBuilder().Set(x => x.Title, title).Build());

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
        [UseAccount(AutomationAccount.SysAdmin)]
        public async Task CreateActivity_CancelButton_ReturnsToList()
        {
            var formPage = new ActivityFormPage(Page);
            await formPage.NavigateToCreateAsync();

            var activitiesPage = await formPage.CancelAsync();

            Assert.Contains("/activities", Page.Url);
            Assert.DoesNotContain("/create", Page.Url);
            _ = activitiesPage;
        }
    }
}
