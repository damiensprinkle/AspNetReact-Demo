namespace Tests.Client.Builders
{
    public class ActivityFormDtoBuilder : BuilderBase<ActivityFormDto, ActivityFormDtoBuilder>
    {
        protected override ActivityFormDto Defaults()
        {
            return new()
            {
                Title       = "Test Activity",
                Description = "A default test activity description.",
                Category    = "culture",
                City        = "London",
                Venue       = "Test Venue",
                Date        = DateTimeOffset.UtcNow.AddMonths(1),
            };
        }
    }
}
