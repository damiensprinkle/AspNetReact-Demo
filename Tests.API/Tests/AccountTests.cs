using Tests.Client;
using Tests.API.Fixtures;

namespace Tests.API.Tests;

[Collection("Api")]
public class AccountTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task Register_WithValidData_ReturnsUser()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];

        var result = await Anon.RegisterAsync(new RegisterDto
        {
            Email       = $"{unique}@test.com",
            Password    = TestPassword,
            Username    = unique,
            DisplayName = "Reg User",
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token!);
        Assert.Equal(unique, result.Username);
    }

    [Fact]
    public async Task Login_WithValidCredentials_ReturnsToken()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];

        await Anon.RegisterAsync(new RegisterDto
        {
            Email       = $"{unique}@test.com",
            Password    = TestPassword,
            Username    = unique,
            DisplayName = "Login User",
        });

        var result = await Anon.LoginAsync(new LoginDto
        {
            Email    = $"{unique}@test.com",
            Password = TestPassword,
        });

        Assert.NotNull(result);
        Assert.NotEmpty(result.Token!);
    }

    [Fact]
    public async Task Login_WithBadPassword_Throws()
    {
        await Assert.ThrowsAsync<ApiException>(() =>
            Anon.LoginAsync(new LoginDto
            {
                Email    = "nobody@nowhere.com",
                Password = "wrongpassword",
            }));
    }

    [Fact]
    public async Task GetCurrentUser_WhenAuthenticated_ReturnsUser()
    {
        var result = await Auth.GetCurrentUserAsync();

        Assert.NotNull(result);
        Assert.Equal(TestDisplay, result.DisplayName);
    }

    [Fact]
    public async Task GetCurrentUser_WhenAnonymous_Throws401()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() => Anon.GetCurrentUserAsync());
        Assert.Equal(401, ex.StatusCode);
    }
}
