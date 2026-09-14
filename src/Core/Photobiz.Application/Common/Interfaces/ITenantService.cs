using Photobiz.Domain.Entities.Master;

namespace Photobiz.Application.Common.Interfaces
{
    /// <summary>
    /// Resolves which tenant database a request belongs to. Implemented in the web layer
    /// (needs the current <c>HttpContext</c> and the Master database) and consumed by
    /// <c>TenantSelectionMiddleware</c> to point the ambient tenant <c>DbContext</c> connection
    /// at the right database before any handler runs.
    /// </summary>
    public interface ITenantService
    {
        /// <summary>
        /// The tenant key carried in the current request's authenticated JWT, or <c>null</c> if
        /// there is no authenticated principal (anonymous request, or a token predating tenancy).
        /// </summary>
        string? GetCurrentTenantKey();

        /// <summary>
        /// Looks up the connection string for a tenant by its public key (the value submitted at
        /// login, and the one embedded in the JWT afterwards). Returns <c>null</c> when no tenant
        /// with that key is registered.
        /// </summary>
        Task<string?> GetTenantConnectionStringAsync(string tenantKey, CancellationToken cancellationToken = default);

        /// <summary>
        /// Resolves a tenant for anonymous traffic (a tenant's public portfolio site) from the
        /// request's Host header: a verified <see cref="Tenant.CustomDomain"/> is matched exactly;
        /// otherwise the leftmost label of a "{tenantKey}.{TenancySettings.BaseDomain}" host is
        /// treated as the tenant key. Returns <c>null</c> when neither resolves.
        /// </summary>
        Task<string?> GetTenantConnectionStringByHostAsync(string host, CancellationToken cancellationToken = default);
    }
}
