namespace Photobiz.Application.Common.Settings
{
    /// <summary>
    /// Where uploaded files live on disk and how they're served back over HTTP. Tenant-scoped
    /// files are written under "{RootPath}/Tenant/{tenantKey}/..." (e.g. logos under
    /// ".../Photos/Logo/") and exposed at "{PublicPathPrefix}/Tenant/{tenantKey}/...".
    /// </summary>
    public class FileStorageSettings
    {
        /// <summary>e.g. "C:\Photobiz" — the physical folder <c>TenantSelectionMiddleware</c>-resolved uploads are written under.</summary>
        public required string RootPath { get; set; }

        /// <summary>e.g. "/media" — the URL prefix <c>Program.cs</c> maps to <see cref="RootPath"/> via static file serving.</summary>
        public string PublicPathPrefix { get; set; } = "/media";
    }
}
