namespace Tests.Client.Builders
{
    public class LoginDtoBuilder : BuilderBase<LoginDto, LoginDtoBuilder>
    {
        protected override LoginDto Defaults()
        {
            return new()
            {
                Email    = "sysadmin1@test.com",
                Password = "Pa$w0rd",
            };
        }
    }
}
