using System.Collections.Concurrent;
using Tests.Client.Builders;

namespace Tests.Client.Mothers
{
    /// <summary>
    /// Object Mother for <see cref="UserDto"/>.
    /// Each named instance is registered via the API exactly once — subsequent calls for the same key
    /// return the cached <see cref="UserDto"/> without hitting the API again.
    /// </summary>
    public class UserMother
    {
        private readonly IClient _client;
        private readonly ConcurrentDictionary<string, UserDto> _cache = new();

        public UserMother(IClient client)
        {
            _client = client;
        }

        /// <summary>Registers (or returns the cached) default test user.</summary>
        public Task<UserDto> DefaultAsync()
        {
            return GetOrCreateAsync("default", () => new RegisterDtoBuilder().Build());
        }

        /// <summary>Registers (or returns the cached) sys-admin test user.</summary>
        public Task<UserDto> SysAdminAsync()
        {
            return GetOrCreateAsync("sysadmin", () => new RegisterDtoBuilder()
                .Set(x => x.DisplayName, "Sys Admin")
                .Set(x => x.Email,       "sysadmin1@test.com")
                .Set(x => x.Username,    "sysadmin1")
                .Build());
        }

        /// <summary>
        /// Returns the cached <see cref="UserDto"/> for <paramref name="key"/> if it exists,
        /// otherwise registers via the API using the DTO returned by <paramref name="factory"/>,
        /// caches the result, and returns it.
        /// </summary>
        public async Task<UserDto> GetOrCreateAsync(string key, Func<RegisterDto> factory)
        {
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            var user = await _client.RegisterAsync(factory());
            _cache.TryAdd(key, user);
            return user;
        }
    }
}
