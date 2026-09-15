using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Photobiz.PortfolioApp.Models;

namespace Photobiz.PortfolioApp.Services
{
    /// <summary>
    /// The only way this app talks to tenant data — over HTTP to Photobiz.Api, exactly like a real
    /// third-party consumer of the platform would. No project reference to Domain/Application/
    /// Infrastructure, deliberately (see Photobiz.PortfolioSample's PortfolioApiClient for the same
    /// rationale): a tenant's public site is not a trusted part of the platform's backend.
    /// </summary>
    public sealed class PortfolioApiClient
    {
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
        {
            Converters = { new JsonStringEnumConverter() }
        };

        private readonly HttpClient _httpClient;

        public PortfolioApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Fetches the tenant's public site payload for the given host. The API resolves tenancy
        /// entirely from the <c>Host</c> header (see <c>TenantSelectionMiddleware</c>'s public-path
        /// branch), so this request must carry the *visitor's* host, not this server's own —
        /// overriding <see cref="HttpRequestMessage.Headers"/>.Host is the standard way to do that
        /// without this app's own outbound connection needing to originate from that domain.
        /// Returns <c>null</c> when the host doesn't resolve to any tenant.
        /// </summary>
        public async Task<PublicSite?> GetSiteAsync(string host, CancellationToken cancellationToken = default)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/api/public/site");

            if (!string.IsNullOrWhiteSpace(host))
            {
                request.Headers.Host = host.Trim();
            }

            using var response = await _httpClient.SendAsync(request, cancellationToken);

            if (response.StatusCode is HttpStatusCode.NotFound or HttpStatusCode.BadRequest)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            return await response.Content.ReadFromJsonAsync<PublicSite>(JsonOptions, cancellationToken);
        }
    }
}
