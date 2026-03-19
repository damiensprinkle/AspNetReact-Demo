using System.Collections.Concurrent;
using Tests.Client.Builders;

namespace Tests.Client.Mothers
{
    /// <summary>
    /// Object Mother for <see cref="ActivityDto"/>.
    /// Each named instance is created via the API exactly once — subsequent calls for the same key
    /// return the cached <see cref="ActivityDto"/> without hitting the API again.
    /// </summary>
    public class ActivityMother
    {
        private readonly IClient _client;
        private readonly ConcurrentDictionary<string, ActivityDto> _cache = new();

        public ActivityMother(IClient client)
        {
            _client = client;
        }

        /// <summary>Creates (or returns the cached) default activity.</summary>
        public Task<ActivityDto> DefaultAsync() =>
            GetOrCreateAsync("default", () => new ActivityFormDtoBuilder().Build());

        /// <summary>Creates (or returns the cached) activity set one year in the future.</summary>
        public Task<ActivityDto> FutureAsync() =>
            GetOrCreateAsync("future", () => new ActivityFormDtoBuilder()
                .Set(x => x.Title, "Future Activity")
                .Set(x => x.Date, DateTimeOffset.UtcNow.AddYears(1))
                .Build());

        /// <summary>
        /// Returns the cached <see cref="ActivityDto"/> for <paramref name="key"/> if it exists,
        /// otherwise calls the API using the DTO returned by <paramref name="factory"/>, caches the
        /// result, and returns it.
        /// </summary>
        public async Task<ActivityDto> GetOrCreateAsync(string key, Func<ActivityFormDto> factory)
        {
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var activity = await _client.CreateActivityAsync(factory());
            _cache.TryAdd(key, activity);
            return activity;
        }

        /// <summary>
        /// Creates a new activity from <paramref name="dto"/> via the API every time — no caching.
        /// Use this for one-off test data that must not be shared between tests.
        /// </summary>
        public Task<ActivityDto> CreateOnceAsync(ActivityFormDto dto) =>
            _client.CreateActivityAsync(dto);
    }
}
