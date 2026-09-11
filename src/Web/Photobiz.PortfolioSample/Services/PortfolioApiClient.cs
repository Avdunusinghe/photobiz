using System.Net.Http.Json;
using System.Text.Json;

namespace Photobiz.PortfolioSample.Services
{
    /// <summary>
    /// A thin client for the real Photobiz API — no shared code with it, deliberately, since a
    /// real tenant portfolio site would be a completely separate application talking to the API
    /// only over HTTP.
    /// </summary>
    public sealed class PortfolioApiClient
    {
        private readonly HttpClient _httpClient;

        public PortfolioApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        /// <summary>
        /// Calls the API's public diagnostics endpoint as if this request had arrived on
        /// <paramref name="simulatedHost"/>. Overriding <c>HttpRequestMessage.Headers.Host</c> is
        /// the standard, supported way to test Host-based routing without real DNS pointed at this
        /// machine — the TCP connection still goes to the API's actual address, only the
        /// transmitted <c>Host:</c> header (what <c>TenantSelectionMiddleware</c> reads) changes.
        /// </summary>
        public Task<ApiCallResult> CheckTenantAsync(string simulatedHost, CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/public/whoami");

            if (!string.IsNullOrWhiteSpace(simulatedHost))
            {
                request.Headers.Host = simulatedHost.Trim();
            }

            return SendAsync(request, cancellationToken);
        }

        /// <summary>Exercises the *other* resolution path: the tenant key travels in the request body, not the Host header.</summary>
        public Task<ApiCallResult> LoginAsync(
            string tenantKey,
            string username,
            string password,
            CancellationToken cancellationToken = default)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/auth/token")
            {
                Content = JsonContent.Create(new { tenantKey, username, password })
            };

            return SendAsync(request, cancellationToken);
        }

        private async Task<ApiCallResult> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            try
            {
                using var response = await _httpClient.SendAsync(request, cancellationToken);
                var body = await response.Content.ReadAsStringAsync(cancellationToken);

                return new ApiCallResult(response.IsSuccessStatusCode, (int)response.StatusCode, PrettyPrintIfJson(body));
            }
            catch (HttpRequestException ex)
            {
                return new ApiCallResult(false, 0, $"Could not reach the API at {_httpClient.BaseAddress}: {ex.Message}");
            }
            finally
            {
                request.Dispose();
            }
        }

        private static string PrettyPrintIfJson(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
            {
                return "(empty response body)";
            }

            try
            {
                using var document = JsonDocument.Parse(body);
                return JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
            }
            catch (JsonException)
            {
                return body;
            }
        }
    }

    public record ApiCallResult(bool Succeeded, int StatusCode, string Body);
}
