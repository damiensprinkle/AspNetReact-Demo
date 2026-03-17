namespace Tests.E2E.Config
{
    public class TestSettings
    {
        public string BaseUrl { get; set; } = "http://localhost:3000";
        public string ApiUrl { get; set; } = "http://localhost:5000/api";
        public bool Headless { get; set; } = true;
        public int SlowMo { get; set; } = 0;
        public TestUserSettings TestUser { get; set; } = new();
    }

    public class TestUserSettings
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
}
