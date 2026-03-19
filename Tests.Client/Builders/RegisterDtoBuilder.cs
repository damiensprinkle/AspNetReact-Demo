namespace Tests.Client.Builders
{
    public class RegisterDtoBuilder : BuilderBase<RegisterDto, RegisterDtoBuilder>
    {
        protected override RegisterDto Defaults()
        {
            return new()
            {
                DisplayName = "Test User",
                Email       = "testuser@test.com",
                Password    = "Pa$w0rd",
                Username    = "testuser",
            };
        }
    }
}
