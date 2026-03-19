namespace Tests.E2E.Fixtures
{
    /// <summary>
    /// Collection 1 — uses the Pool1 account set (sysadmin1, user1).
    /// Runs in parallel with Playwright.2.
    /// </summary>
    [CollectionDefinition("Playwright.1")]
    public class PlaywrightCollection1 : ICollectionFixture<PlaywrightFixturePool1> { }

    /// <summary>
    /// Collection 2 — uses the Pool2 account set (sysadmin2, user2).
    /// Runs in parallel with Playwright.1.
    /// </summary>
    [CollectionDefinition("Playwright.2")]
    public class PlaywrightCollection2 : ICollectionFixture<PlaywrightFixturePool2> { }
}
