using Microsoft.Extensions.Options;
using Photobiz.Application.Common.Settings;
using Photobiz.Infrastructure.Media;

namespace Photobiz.Infrastructure.Tests.Media
{
    public class TenantLogoStorageTests : IDisposable
    {
        private readonly string _rootPath = Path.Combine(Path.GetTempPath(), "photobiz-tests-" + Guid.NewGuid());
        private readonly TenantLogoStorage _storage;

        public TenantLogoStorageTests()
        {
            var settings = new FileStorageSettings { RootPath = _rootPath, PublicPathPrefix = "/media" };
            _storage = new TenantLogoStorage(Options.Create(settings));
        }

        public void Dispose()
        {
            if (Directory.Exists(_rootPath))
            {
                Directory.Delete(_rootPath, recursive: true);
            }
        }

        [Fact]
        public async Task SaveAsync_WritesTheFileUnderTenantPhotosLogo()
        {
            var result = await _storage.SaveAsync("acme", [1, 2, 3], "acme-logo-abcd1234.webp", "https://api.photobiz.test");

            var expectedPhysicalPath = Path.Combine(_rootPath, "Tenant", "acme", "Photos", "Logo", "acme-logo-abcd1234.webp");
            Assert.True(File.Exists(expectedPhysicalPath));
            Assert.Equal([1, 2, 3], await File.ReadAllBytesAsync(expectedPhysicalPath));
        }

        [Fact]
        public async Task SaveAsync_ReturnsAForwardSlashRelativeStoragePathAndAnAbsolutePublicUrl()
        {
            var result = await _storage.SaveAsync("acme", [1, 2, 3], "acme-logo-abcd1234.webp", "https://api.photobiz.test");

            Assert.Equal("Tenant/acme/Photos/Logo/acme-logo-abcd1234.webp", result.StoragePath);
            Assert.Equal(
                "https://api.photobiz.test/media/Tenant/acme/Photos/Logo/acme-logo-abcd1234.webp",
                result.PublicUrl);
        }

        [Fact]
        public async Task SaveAsync_TrimsATrailingSlashFromTheBaseUrl()
        {
            var result = await _storage.SaveAsync("acme", [1, 2, 3], "acme-logo-abcd1234.webp", "https://api.photobiz.test/");

            Assert.Equal(
                "https://api.photobiz.test/media/Tenant/acme/Photos/Logo/acme-logo-abcd1234.webp",
                result.PublicUrl);
        }

        [Fact]
        public async Task SaveAsync_CreatesTheDirectoryTreeWhenItDoesNotYetExist()
        {
            Assert.False(Directory.Exists(_rootPath));

            await _storage.SaveAsync("acme", [1], "logo.webp", "https://api.photobiz.test");

            Assert.True(Directory.Exists(Path.Combine(_rootPath, "Tenant", "acme", "Photos", "Logo")));
        }

        [Fact]
        public async Task DeleteAsync_RemovesAPreviouslySavedFile()
        {
            await _storage.SaveAsync("acme", [1], "logo.webp", "https://api.photobiz.test");
            var physicalPath = Path.Combine(_rootPath, "Tenant", "acme", "Photos", "Logo", "logo.webp");
            Assert.True(File.Exists(physicalPath));

            await _storage.DeleteAsync("Tenant/acme/Photos/Logo/logo.webp");

            Assert.False(File.Exists(physicalPath));
        }

        [Fact]
        public async Task DeleteAsync_WhenTheFileDoesNotExist_DoesNotThrow()
        {
            var exception = await Record.ExceptionAsync(() =>
                _storage.DeleteAsync("Tenant/acme/Photos/Logo/does-not-exist.webp"));

            Assert.Null(exception);
        }
    }
}
