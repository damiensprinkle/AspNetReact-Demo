namespace Tests.E2E.Fixtures
{
    [CollectionDefinition("Playwright")]
    public class PlaywrightCollection : ICollectionFixture<PlaywrightFixture>
    {
        // This class has no code. It exists only to apply [CollectionDefinition]
        // so all test classes using [Collection("Playwright")] share one browser process.
    }
}
