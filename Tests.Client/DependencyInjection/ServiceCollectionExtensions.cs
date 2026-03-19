using Microsoft.Extensions.DependencyInjection;

namespace Tests.Client.DependencyInjection
{
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Registers an anonymous <see cref="IClient"/> for unauthenticated requests.
        /// </summary>
        public static IServiceCollection AddApiClient(this IServiceCollection services, string baseUrl)
        {
            services.AddSingleton<IClient>(_ => ApiClientFactory.CreateAnonymous(baseUrl));
            return services;
        }

        /// <summary>
        /// Registers an authenticated <see cref="IClient"/> using the provided token factory.
        /// The token factory is called once at registration time.
        /// </summary>
        public static IServiceCollection AddAuthenticatedApiClient(
            this IServiceCollection services,
            string baseUrl,
            Func<string> tokenProvider)
        {
            services.AddSingleton<IClient>(_ => ApiClientFactory.CreateAuthenticated(baseUrl, tokenProvider()));
            return services;
        }
    }
}
