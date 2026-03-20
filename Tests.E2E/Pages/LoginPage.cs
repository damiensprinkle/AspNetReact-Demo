using Microsoft.Playwright;

namespace Tests.E2E.Pages
{
    public class LoginPage(IPage page)
    {
        public ILocator EmailInput    => page.GetByPlaceholder("Email");
        public ILocator PasswordInput => page.GetByPlaceholder("Password");
        public ILocator SubmitButton  => page.Locator("button[type='submit']");
        public ILocator ErrorMessage  => page.Locator("[data-testid='login-error']");

        public async Task LoginAsync(string email, string password)
        {
            await EmailInput.FillAsync(email);
            await PasswordInput.FillAsync(password);
            await SubmitButton.ClickAsync();
        }
    }
}
