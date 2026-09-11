using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Infrastructure.Persistence;

namespace Photobiz.Api.Middleware
{
    /// <summary>
    /// Points the ambient tenant <see cref="PhotobizDbContext"/> at the right database before any
    /// controller/handler runs, resolving the tenant from one of three places depending on the
    /// request:
    /// <list type="bullet">
    /// <item>the request body, for the login endpoint — a JWT already sitting in the browser
    /// carries the <i>previous</i> session's tenant claim, so trusting it here for a fresh login
    /// would silently authenticate against the wrong tenant database instead of the one the user
    /// is actually signing into;</item>
    /// <item>the request's <c>Host</c> header, for anonymous public routes (a tenant's portfolio
    /// site) — there's no JWT at all for a site visitor, so the domain/subdomain they landed on
    /// <i>is</i> the tenant identity (see <c>ITenantService.GetTenantConnectionStringByHostAsync</c>);</item>
    /// <item>the validated JWT's <c>tenant_key</c> claim for every other (authenticated admin)
    /// endpoint — this runs after <c>UseAuthentication()</c>, so the claim is already verified.</item>
    /// </list>
    /// Registered via <c>app.UseMiddleware&lt;TenantSelectionMiddleware&gt;()</c>; exceptions are
    /// deliberately left to propagate to <c>UseExceptionHandler()</c> / <c>GlobalExceptionHandler</c>
    /// rather than being caught here, so failures downstream in the pipeline aren't misreported as
    /// tenant-resolution failures.
    /// </summary>
    public class TenantSelectionMiddleware
    {
        private const string AuthPathPrefix = "/api/auth";
        private const string PublicPathPrefix = "/api/public";

        private static readonly JsonSerializerOptions ProbeSerializerOptions = new()
        {
            PropertyNameCaseInsensitive = true
        };

        private readonly RequestDelegate _next;

        public TenantSelectionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(
            HttpContext httpContext,
            PhotobizDbContext tenantDbContext,
            ITenantService tenantService)
        {
            var resolution = await ResolveAsync(httpContext, tenantService);

            if (resolution is null)
            {
                // Anonymous request to a protected (non-public, non-auth) endpoint, or a token
                // issued before tenancy was added — let [Authorize] reject it; there's nothing
                // tenant-specific to resolve.
                await _next(httpContext);
                return;
            }

            var (identifier, connectionString) = resolution.Value;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new TenantNotFoundException($"No tenant is registered for '{identifier}'.");
            }

            tenantDbContext.Database.SetConnectionString(connectionString);

            await _next(httpContext);
        }

        private static async Task<(string Identifier, string? ConnectionString)?> ResolveAsync(
            HttpContext httpContext,
            ITenantService tenantService)
        {
            var request = httpContext.Request;
            var cancellationToken = httpContext.RequestAborted;

            if (request.Path.StartsWithSegments(AuthPathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                var tenantKey = await ReadTenantKeyFromBodyAsync(httpContext);

                return tenantKey is null
                    ? null
                    : (tenantKey, await tenantService.GetTenantConnectionStringAsync(tenantKey, cancellationToken));
            }

            if (request.Path.StartsWithSegments(PublicPathPrefix, StringComparison.OrdinalIgnoreCase))
            {
                // A site visitor carries no credential at all — the host they landed on (a
                // "{tenantKey}.{baseDomain}" subdomain, or a tenant's own verified custom domain)
                // is the only signal available, so always resolve (never fall through silently).
                var host = request.Host.Host;
                return (host, await tenantService.GetTenantConnectionStringByHostAsync(host, cancellationToken));
            }

            var claimedTenantKey = tenantService.GetCurrentTenantKey();

            return claimedTenantKey is null
                ? null
                : (claimedTenantKey, await tenantService.GetTenantConnectionStringAsync(claimedTenantKey, cancellationToken));
        }

        private static async Task<string?> ReadTenantKeyFromBodyAsync(HttpContext httpContext)
        {
            if (!httpContext.Request.HasJsonContentType())
            {
                return null;
            }

            // Makes the body seekable so it can be read here AND again, from the start, by model
            // binding in the controller.
            httpContext.Request.EnableBuffering();

            try
            {
                var probe = await JsonSerializer.DeserializeAsync<TenantLoginProbe>(
                    httpContext.Request.Body, ProbeSerializerOptions, httpContext.RequestAborted);

                return string.IsNullOrWhiteSpace(probe?.TenantKey) ? null : probe.TenantKey.Trim();
            }
            catch (JsonException)
            {
                // Malformed JSON: this probe only best-effort-extracts one field. Leave rejecting
                // the request to the controller's own model binding, which will hit the same body
                // and return a proper 400.
                return null;
            }
            finally
            {
                httpContext.Request.Body.Position = 0;
            }
        }

        private sealed record TenantLoginProbe(string? TenantKey);
    }
}
