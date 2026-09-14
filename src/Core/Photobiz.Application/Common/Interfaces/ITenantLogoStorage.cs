namespace Photobiz.Application.Common.Interfaces
{
    public record StoredTenantLogo(string StoragePath, string PublicUrl);

    /// <summary>
    /// Writes/removes a tenant's logo file under "Tenant/{tenantKey}/Photos/Logo/" on the
    /// configured storage root and turns that into a URL the browser can load the image from.
    /// </summary>
    public interface ITenantLogoStorage
    {
        /// <summary>
        /// Saves <paramref name="content"/> as the tenant's logo, replacing whatever file (if any)
        /// previously lived at that path. <paramref name="fileName"/> should already include the
        /// extension (e.g. "acme-logo-3f1a9c2e.webp").
        /// </summary>
        Task<StoredTenantLogo> SaveAsync(
            string tenantKey,
            byte[] content,
            string fileName,
            string baseUrl,
            CancellationToken cancellationToken = default);

        /// <summary>Deletes the file at <paramref name="storagePath"/> if it exists; a no-op otherwise (e.g. already removed).</summary>
        Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default);
    }
}
