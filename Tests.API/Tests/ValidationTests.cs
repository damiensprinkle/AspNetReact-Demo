using Tests.Client;
using Tests.API.Fixtures;

namespace Tests.API.Tests;

[Collection("Api")]
public class ValidationTests(ApiFactory factory) : ApiTestBase(factory)
{
    [Fact]
    public async Task CreateActivity_WithEmptyTitle_Throws400()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            Auth.CreateActivityAsync(new ActivityFormDto
            {
                Title       = "",
                Date        = DateTimeOffset.UtcNow.AddDays(1),
                Description = "desc",
                Category    = "culture",
                City        = "London",
                Venue       = "Venue",
            }));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task CreateActivity_WithMissingDate_Throws400()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            Auth.CreateActivityAsync(new ActivityFormDto
            {
                Title       = "Title",
                Description = "desc",
                Category    = "culture",
                City        = "London",
                Venue       = "Venue",
            }));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Throws400()
    {
        var unique = Guid.NewGuid().ToString("N")[..8];

        await Anon.RegisterAsync(new RegisterDto
        {
            Email       = $"{unique}@test.com",
            Password    = TestPassword,
            Username    = unique + "a",
            DisplayName = "User A",
        });

        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            Anon.RegisterAsync(new RegisterDto
            {
                Email       = $"{unique}@test.com",
                Password    = TestPassword,
                Username    = unique + "b",
                DisplayName = "User B",
            }));

        Assert.Equal(400, ex.StatusCode);
    }
}
