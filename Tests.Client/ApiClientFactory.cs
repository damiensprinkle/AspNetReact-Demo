using System.Net.Http.Headers;

namespace Tests.Client
{
    /// <summary>
    /// Creates typed <see cref="IClient"/> instances backed by the NSwag-generated client.
    /// <list type="bullet">
    ///   <item><see cref="CreateAnonymous"/> / <see cref="CreateAuthenticated"/> — for live servers (E2E tests).</item>
    ///   <item><see cref="Wrap"/> — for in-process <c>HttpClient</c> instances from <c>WebApplicationFactory</c> (API tests).</item>
    /// </list>
    /// </summary>
    public static class ApiClientFactory
    {
        /// <summary>Creates an anonymous typed client for a live server.</summary>
        public static IClient CreateAnonymous(string baseUrl)
        {
            return new Client(Normalize(baseUrl), new HttpClient());
        }

        /// <summary>Creates a Bearer-authenticated typed client for a live server.</summary>
        public static IClient CreateAuthenticated(string baseUrl, string token)
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", token);
            return new Client(Normalize(baseUrl), http);
        }

        /// <summary>
        /// Wraps an existing <see cref="HttpClient"/> with the typed client.
        /// Used by Tests.API where <c>WebApplicationFactory.CreateClient()</c> provides
        /// a pre-configured in-process client.
        /// </summary>
        public static IClient Wrap(string baseUrl, HttpClient httpClient)
        {
            return new Client(baseUrl, httpClient);
        }

        private static string Normalize(string url)
        {
            return url.EndsWith('/') ? url : url + "/";
        }
    }
}
