using Microsoft.Extensions.Options;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Settings;

namespace Photobiz.Infrastructure.Media
{
    /// <summary>
    /// Writes logo files under "{RootPath}/Tenant/{tenantKey}/Photos/Logo/" — e.g., with the default
    /// dev settings, "C:\Photobiz\Tenant\default\Photos\Logo\default-logo-3f1a9c2e.webp".
    /// </summary>
    public sealed class TenantLogoStorage : ITenantLogoStorage
    {
        private readonly FileStorageSettings _settings;

        public TenantLogoStorage(IOptions<FileStorageSettings> settings)
        {
            _settings = settings.Value;
        }

        public async Task<StoredTenantLogo> SaveAsync(
            string tenantKey,
            byte[] content,
            string fileName,
            string baseUrl,
            CancellationToken cancellationToken = default)
        {
            var relativePath = BuildRelativePath(tenantKey, fileName);
            var physicalPath = ToPhysicalPath(relativePath);

            Directory.CreateDirectory(Path.GetDirectoryName(physicalPath)!);
            await File.WriteAllBytesAsync(physicalPath, content, cancellationToken);

            var publicUrl = $"{baseUrl.TrimEnd('/')}{_settings.PublicPathPrefix}/{relativePath}";

            return new StoredTenantLogo(relativePath, publicUrl);
        }

        public Task DeleteAsync(string storagePath, CancellationToken cancellationToken = default)
        {
            var physicalPath = ToPhysicalPath(storagePath);

            if (File.Exists(physicalPath))
            {
                File.Delete(physicalPath);
            }

            return Task.CompletedTask;
        }

        /// <summary>Forward slashes throughout — this is also the URL path, not just a file path.</summary>
        private static string BuildRelativePath(string tenantKey, string fileName) =>
            $"Tenant/{tenantKey}/Photos/Logo/{fileName}";

        private string ToPhysicalPath(string relativePath) =>
            Path.Combine(_settings.RootPath, relativePath.Replace('/', Path.DirectorySeparatorChar));
    }
}
