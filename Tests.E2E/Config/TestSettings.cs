namespace Tests.E2E.Config
{
    public class TestSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:3000";
        public string ApiUrl { get; set; } = "http://localhost:5000/api";
        public bool Headless { get; set; } = true;
        public int SlowMo { get; set; } = 0;

        /// <summary>
        /// Outer key: pool name ("Pool1", "Pool2").
        /// Inner key: account role name ("SysAdmin", "StandardUser").
        /// Each parallel collection is assigned exactly one pool so concurrent tests
        /// never share credentials.
        /// </summary>
        public Dictionary<string, Dictionary<string, TestUserSettings>> Pools { get; set; } = new();

        public TestUserSettings GetAccount(string poolName, AutomationAccount account)
        {
            if (!Pools.TryGetValue(poolName, out var pool))
                throw new InvalidOperationException(
                    $"Pool '{poolName}' not found in testsettings.json > Pools.");

            if (!pool.TryGetValue(account.ToString(), out var user))
                throw new InvalidOperationException(
                    $"Account '{account}' not found in pool '{poolName}'. Check testsettings.json.");

            return user;
        }
    }

    public class TestUserSettings
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
}
