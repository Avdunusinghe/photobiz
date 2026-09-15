using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Photobiz.Application.Common.Constants;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Settings;
using Photobiz.Domain.Entities.Master;
using Photobiz.Infrastructure.Persistence;

namespace Photobiz.Api.Services
{
    /// <summary>
    /// Looks up tenants in the Master database. Lives in the web layer (rather than
    /// Infrastructure) because it needs <see cref="IHttpContextAccessor"/> — the same reason
    /// <see cref="CurrentUser"/> does — alongside <see cref="MasterDbContext"/>, which the API
    /// project already references.
    /// </summary>
    public sealed class TenantService : ITenantService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly MasterDbContext _masterDbContext;
        private readonly TenancySettings _tenancySettings;

        public TenantService(
            IHttpContextAccessor httpContextAccessor,
            MasterDbContext masterDbContext,
            IOptions<TenancySettings> tenancySettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _masterDbContext = masterDbContext;
            _tenancySettings = tenancySettings.Value;
        }

        public string? GetCurrentTenantKey()
        {
            var principal = _httpContextAccessor.HttpContext?.User;

            if (principal?.Identity?.IsAuthenticated != true)
            {
                return null;
            }

            return principal.FindFirstValue(TenantClaimTypes.TenantKey);
        }

        public async Task<string?> GetTenantConnectionStringAsync(
            string tenantKey,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(tenantKey))
            {
                return null;
            }

            // Not gated on IsSubscribed / SubscriptionExpiredOn yet — once billing is wired up,
            // add that check here so a lapsed subscription blocks access at the same choke point
            // every request already passes through.
            return await _masterDbContext.Tenants
                .Where(t => t.TenantKey == tenantKey)
                .Select(t => t.ConnectionString)
                .SingleOrDefaultAsync(cancellationToken);
        }

        public async Task<string?> GetTenantConnectionStringByHostAsync(
            string host,
            CancellationToken cancellationToken = default)
        {
            var tenant = await ResolveTenantByHostAsync(host, cancellationToken);

            return tenant?.ConnectionString;
        }

        public async Task<PublicTenantInfo?> GetPublicTenantByHostAsync(
            string host,
            CancellationToken cancellationToken = default)
        {
            var tenant = await ResolveTenantByHostAsync(host, cancellationToken);

            return tenant is null ? null : new PublicTenantInfo(tenant.Name, tenant.LogoUrl);
        }

        private async Task<Tenant?> ResolveTenantByHostAsync(string host, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(host))
            {
                return null;
            }

            var normalizedHost = host.Trim().ToLowerInvariant();

            // A verified custom domain always wins — it's an explicit, individually-claimed
            // identity, unlike a subdomain that's mechanically derived from the base domain.
            var byCustomDomain = await _masterDbContext.Tenants
                .SingleOrDefaultAsync(
                    t => t.CustomDomain == normalizedHost && t.CustomDomainVerifiedAt != null,
                    cancellationToken);

            if (byCustomDomain is not null)
            {
                return byCustomDomain;
            }

            var tenantKey = ExtractSubdomainTenantKey(normalizedHost);

            return tenantKey is null
                ? null
                : await _masterDbContext.Tenants.SingleOrDefaultAsync(t => t.TenantKey == tenantKey, cancellationToken);
        }

        private string? ExtractSubdomainTenantKey(string normalizedHost)
        {
            var baseDomain = _tenancySettings.BaseDomain.Trim().ToLowerInvariant();

            if (string.IsNullOrEmpty(baseDomain))
            {
                return null;
            }

            var suffix = "." + baseDomain;

            return normalizedHost.Length > suffix.Length &&
                normalizedHost.EndsWith(suffix, StringComparison.Ordinal)
                ? normalizedHost[..^suffix.Length]
                : null;
        }
    }
}
