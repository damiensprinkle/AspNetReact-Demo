using Microsoft.Playwright;

namespace Tests.E2E.Pages
{
    /// <summary>Interactions with the fixed top navigation bar.</summary>
    public class NavBarPage(IPage page)
    {
        public ILocator LoginButton          => page.Locator("[data-testid='login-button']");
        public ILocator RegisterButton       => page.Locator("[data-testid='register-button']");
        public ILocator LogoutButton         => page.Locator("[data-testid='logout-button']");
        public ILocator CreateActivityButton => page.Locator("[data-testid='create-activity-button']");
        public ILocator ActivitiesLink       => page.Locator("[data-testid='activities-link']");

        public async Task ClickLoginAsync()          => await LoginButton.ClickAsync();
        public async Task ClickRegisterAsync()       => await RegisterButton.ClickAsync();
        public async Task ClickLogoutAsync()         => await LogoutButton.ClickAsync();
        public async Task ClickCreateActivityAsync() => await CreateActivityButton.ClickAsync();
        public async Task ClickActivitiesAsync()     => await ActivitiesLink.ClickAsync();

        public Task<bool> IsLoggedInAsync() => LogoutButton.IsVisibleAsync();
    }
}
