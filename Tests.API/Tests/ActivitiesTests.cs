using Tests.Client;
using Tests.API.Fixtures;

namespace Tests.API.Tests;

[Collection("Api")]
public class ActivitiesTests(ApiFactory factory) : ApiTestBase(factory)
{
    private static ActivityFormDto NewActivity(string suffix = "")
    {
        return new()
        {
            Title       = $"Test Activity {suffix}",
            Date        = DateTimeOffset.UtcNow.AddDays(1),
            Description = "Integration test activity",
            Category    = "culture",
            City        = "London",
            Venue       = "Tate Modern",
        };
    }

    [Fact]
    public async Task GetActivities_ReturnsOk()
    {
        var result = await Anon.GetActivitiesAsync();

        Assert.NotNull(result);
    }

    [Fact]
    public async Task CreateActivity_WhenAuthenticated_ReturnsSavedActivity()
    {
        var form    = NewActivity("create");
        var created = await Auth.CreateActivityAsync(form);

        Assert.NotEqual(Guid.Empty, created.Id);
        Assert.Equal(form.Title, created.Title);
        Assert.Equal(form.City,  created.City);
    }

    [Fact]
    public async Task CreateActivity_WhenAnonymous_Throws401()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            Anon.CreateActivityAsync(NewActivity()));

        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task GetActivity_WithValidId_ReturnsActivity()
    {
        var created = await Auth.CreateActivityAsync(NewActivity("get"));
        var result  = await Anon.GetActivityAsync(created.Id);

        Assert.Equal(created.Id,    result.Id);
        Assert.Equal(created.Title, result.Title);
    }

    [Fact]
    public async Task GetActivity_WithInvalidId_Throws404()
    {
        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            Anon.GetActivityAsync(Guid.NewGuid()));

        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task EditActivity_WhenAuthenticated_ReturnsUpdatedActivity()
    {
        var created = await Auth.CreateActivityAsync(NewActivity("edit-before"));

        var updated = await Auth.EditActivityAsync(
            created.Id,
            new ActivityFormDto
            {
                Title       = "Updated Title",
                Date        = created.Date,
                Description = created.Description!,
                Category    = created.Category!,
                City        = created.City!,
                Venue       = created.Venue!,
            });

        Assert.Equal("Updated Title", updated.Title);
        Assert.Equal(created.Id,      updated.Id);
    }

    [Fact]
    public async Task EditActivity_WhenAnonymous_Throws401()
    {
        var created = await Auth.CreateActivityAsync(NewActivity("edit-anon"));

        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            Anon.EditActivityAsync(created.Id, NewActivity("updated")));

        Assert.Equal(401, ex.StatusCode);
    }

    [Fact]
    public async Task DeleteActivity_WhenAuthenticated_RemovesActivity()
    {
        var created = await Auth.CreateActivityAsync(NewActivity("delete"));

        await Auth.DeleteActivityAsync(created.Id);

        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            Anon.GetActivityAsync(created.Id));
        Assert.Equal(404, ex.StatusCode);
    }

    [Fact]
    public async Task DeleteActivity_WhenAnonymous_Throws401()
    {
        var created = await Auth.CreateActivityAsync(NewActivity("delete-anon"));

        var ex = await Assert.ThrowsAsync<ApiException>(() =>
            Anon.DeleteActivityAsync(created.Id));

        Assert.Equal(401, ex.StatusCode);
    }
}
