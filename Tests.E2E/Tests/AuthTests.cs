using Tests.E2E.Config;
using Tests.E2E.Fixtures;
using Tests.E2E.Pages;
using Xunit.Abstractions;

namespace Tests.E2E.Tests
{
    [Collection("Playwright.2")]
    public class AuthTests : PageTestBase
    {
        public AuthTests(PlaywrightFixturePool2 fixture, ITestOutputHelper output)
            : base(fixture, output) { }

        [Fact]
        public async Task Login_WithValidCredentials_RedirectsToDashboard()
        {
            var account = GetAccount(AutomationAccount.SysAdmin);
            await Page.GotoAsync("/login");
            var loginPage = new LoginPage(Page);

            await loginPage.LoginAsync(account.Email, account.Password);

            await Page.WaitForURLAsync("**/activities");
            Assert.Contains("/activities", Page.Url);
        }

        [Fact]
        public async Task Login_WithInvalidCredentials_ShowsError()
        {
            await Page.GotoAsync("/login");
            var loginPage = new LoginPage(Page);

            await loginPage.LoginAsync("wrong@email.com", "WrongPass1");

            var error = loginPage.ErrorMessage;
            await error.WaitForAsync();
            Assert.True(await error.IsVisibleAsync());
        }

        [Fact]
        public async Task Register_WithNewUser_RedirectsToDashboard()
        {
            var unique = Guid.NewGuid().ToString("N")[..8];
            await Page.GotoAsync("/register");
            var registerPage = new RegisterPage(Page);

            await registerPage.RegisterAsync(
                displayName: $"User {unique}",
                username:    $"user{unique}",
                email:       $"{unique}@test.com",
                password:    "Pa$$w0rd");

            await Page.WaitForURLAsync("**/activities");
            Assert.Contains("/activities", Page.Url);
        }

        [Fact]
        public async Task Logout_ClearsSessionAndShowsLoginButton()
        {
            await LoginAsTestUserAsync();
            await Page.GotoAsync("/activities");

            var navBar = new NavBarPage(Page);
            Assert.True(await navBar.IsLoggedInAsync());

            await navBar.ClickLogoutAsync();

            await Page.WaitForLoadStateAsync(Microsoft.Playwright.LoadState.NetworkIdle);
            Assert.True(await navBar.LoginButton.IsVisibleAsync());
        }

        [Fact]
        public async Task NavBar_WhenLoggedOut_ShowsLoginAndRegisterButtons()
        {
            await Page.GotoAsync("/activities");
            var navBar = new NavBarPage(Page);

            Assert.True(await navBar.LoginButton.IsVisibleAsync());
            Assert.True(await navBar.RegisterButton.IsVisibleAsync());
        }

        [Fact]
        public async Task NavBar_WhenLoggedIn_ShowsLogoutAndCreateButtons()
        {
            await LoginAsTestUserAsync();
            await Page.GotoAsync("/activities");

            var navBar = new NavBarPage(Page);
            Assert.True(await navBar.IsLoggedInAsync());
            Assert.True(await navBar.CreateActivityButton.IsVisibleAsync());
        }
    }
}
