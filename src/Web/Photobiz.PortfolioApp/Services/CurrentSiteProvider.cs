using Microsoft.AspNetCore.Http;
using Photobiz.PortfolioApp.Models;

namespace Photobiz.PortfolioApp.Services
{
    /// <summary>Thrown when the request's Host header doesn't resolve to any tenant.</summary>
    public sealed class SiteNotFoundException : Exception
    {
        public SiteNotFoundException(string host) : base($"No tenant site is configured for host '{host}'.")
        {
        }
    }

    /// <summary>
    /// Fetches the current request's tenant site data once and memoizes it for the rest of the
    /// request — every controller action and the shared layout need it, and it's one HTTP round
    /// trip to Photobiz.Api, not one per consumer.
    /// </summary>
    public interface ICurrentSiteProvider
    {
        /// <exception cref="SiteNotFoundException">The request's host has no matching tenant.</exception>
        Task<PublicSite> GetAsync(CancellationToken cancellationToken = default);
    }

    public sealed class CurrentSiteProvider : ICurrentSiteProvider
    {
        private static readonly string[] LocalHostNames = ["localhost", "127.0.0.1", "[::1]", "::1"];

        private readonly PortfolioApiClient _apiClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHostEnvironment _environment;
        private readonly IConfiguration _configuration;
        private Task<PublicSite>? _cached;

        public CurrentSiteProvider(
            PortfolioApiClient apiClient,
            IHttpContextAccessor httpContextAccessor,
            IHostEnvironment environment,
            IConfiguration configuration)
        {
            _apiClient = apiClient;
            _httpContextAccessor = httpContextAccessor;
            _environment = environment;
            _configuration = configuration;
        }

        public Task<PublicSite> GetAsync(CancellationToken cancellationToken = default) =>
            _cached ??= FetchAsync(cancellationToken);

        private async Task<PublicSite> FetchAsync(CancellationToken cancellationToken)
        {
            var host = _httpContextAccessor.HttpContext?.Request.Host.Host ?? string.Empty;

            // Pressing F5 (or `dotnet run`) hits plain "localhost" — that never resolves to a real
            // tenant (by design: the same as any stranger's unrecognized domain in production), so
            // local development would otherwise always show "site not found". Substituting a
            // configured tenant host here — only in Development, only for localhost/loopback — lets
            // running the app locally show real content without a hosts-file edit.
            if (_environment.IsDevelopment() && LocalHostNames.Contains(host, StringComparer.OrdinalIgnoreCase))
            {
                var devHost = _configuration["PortfolioApp:DevelopmentTenantHost"];
                if (!string.IsNullOrWhiteSpace(devHost))
                {
                    host = devHost;
                }
            }

            var site = await _apiClient.GetSiteAsync(host, cancellationToken);

            return site ?? throw new SiteNotFoundException(host);
        }
    }
}
