using Microsoft.Playwright;

namespace Tests.E2E.Pages
{
    public class RegisterPage(IPage page)
    {
        public ILocator DisplayNameInput => page.GetByPlaceholder("Display Name");
        public ILocator UsernameInput    => page.GetByPlaceholder("Username");
        public ILocator EmailInput       => page.GetByPlaceholder("Email");
        public ILocator PasswordInput    => page.GetByPlaceholder("Password");
        public ILocator SubmitButton     => page.Locator("button[type='submit']");
        public ILocator ErrorMessage     => page.Locator("[data-testid='register-error']");

        public async Task RegisterAsync(string displayName, string username, string email, string password)
        {
            await DisplayNameInput.FillAsync(displayName);
            await UsernameInput.FillAsync(username);
            await EmailInput.FillAsync(email);
            await PasswordInput.FillAsync(password);
            await SubmitButton.ClickAsync();
        }
    }
}
