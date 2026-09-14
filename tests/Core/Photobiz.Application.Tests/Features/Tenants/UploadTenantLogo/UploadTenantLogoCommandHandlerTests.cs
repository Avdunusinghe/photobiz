using FluentValidation;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Features.Tenants.UploadTenantLogo;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Entities.Master;

namespace Photobiz.Application.Tests.Features.Tenants.UploadTenantLogo
{
    public class UploadTenantLogoCommandHandlerTests
    {
        private readonly InMemoryMasterDbContext _dbContext = InMemoryMasterDbContext.Create();
        private readonly ITenantService _tenantService = Substitute.For<ITenantService>();
        private readonly IImageProcessor _imageProcessor = Substitute.For<IImageProcessor>();
        private readonly ITenantLogoStorage _logoStorage = Substitute.For<ITenantLogoStorage>();
        private readonly IPublicUrlProvider _publicUrlProvider = Substitute.For<IPublicUrlProvider>();
        private readonly UploadTenantLogoCommandHandler _handler;

        public UploadTenantLogoCommandHandlerTests()
        {
            _handler = new UploadTenantLogoCommandHandler(
                _dbContext, _tenantService, _imageProcessor, _logoStorage, _publicUrlProvider, TestMappingConfig.Create());

            _publicUrlProvider.GetBaseUrl().Returns("https://api.photobiz.test");
        }

        private static UploadTenantLogoCommand Command() => new(
            Content: new MemoryStream([1, 2, 3]),
            FileName: "logo.png",
            ContentType: "image/png",
            Length: 3);

        private async Task<Tenant> SeedTenantAsync(string tenantKey = "acme")
        {
            var tenant = Tenant.Create(
                tenantKey, "Acme Studio", "Server=.;Database=Tenant_Acme;",
                "owner@acme.test", "Ada", "Lovelace", "+1 555 0100", "1 Street", "London", "UK",
                "pro", "monthly", Guid.NewGuid());
            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync();
            _tenantService.GetCurrentTenantKey().Returns(tenantKey);
            return tenant;
        }

        private void StubSuccessfulConversion(byte[]? content = null) =>
            _imageProcessor
                .ConvertToWebPAsync(Arg.Any<Stream>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns(new ProcessedImage(content ?? [9, 9, 9], "image/webp", "webp", 512, 512));

        [Fact]
        public async Task Handle_SavesTheConvertedImageAndUpdatesTheTenant()
        {
            var tenant = await SeedTenantAsync();
            var convertedContent = new byte[] { 4, 5, 6 };
            StubSuccessfulConversion(convertedContent);
            _logoStorage
                .SaveAsync("acme", convertedContent, Arg.Any<string>(), "https://api.photobiz.test", Arg.Any<CancellationToken>())
                .Returns(new StoredTenantLogo(
                    "Tenant/acme/Photos/Logo/acme-logo-abcd1234.webp",
                    "https://api.photobiz.test/media/Tenant/acme/Photos/Logo/acme-logo-abcd1234.webp"));

            var result = await _handler.Handle(Command(), CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal("Logo uploaded successfully.", result.Message);
            Assert.Equal(
                "https://api.photobiz.test/media/Tenant/acme/Photos/Logo/acme-logo-abcd1234.webp",
                result.Data!.LogoUrl);

            var stored = await _dbContext.Tenants.SingleAsync(t => t.Id == tenant.Id);
            Assert.Equal(
                "https://api.photobiz.test/media/Tenant/acme/Photos/Logo/acme-logo-abcd1234.webp",
                stored.LogoUrl);
            Assert.Equal("Tenant/acme/Photos/Logo/acme-logo-abcd1234.webp", stored.LogoStoragePath);
        }

        [Fact]
        public async Task Handle_GeneratesASlugifiedFileNameRatherThanTheOriginal()
        {
            await SeedTenantAsync();
            StubSuccessfulConversion();
            _logoStorage
                .SaveAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new StoredTenantLogo("path.webp", "https://api.photobiz.test/media/path.webp"));

            await _handler.Handle(Command(), CancellationToken.None);

            await _logoStorage.Received(1).SaveAsync(
                "acme",
                Arg.Any<byte[]>(),
                Arg.Is<string>(name => name.StartsWith("acme-logo-") && name.EndsWith(".webp") && !name.Contains("logo.png")),
                "https://api.photobiz.test",
                Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenAPreviousLogoExists_DeletesItOnlyAfterSavingSucceeds()
        {
            var tenant = await SeedTenantAsync();
            tenant.SetLogo("https://old.test/logo.webp", "Tenant/acme/Photos/Logo/old.webp");
            await _dbContext.SaveChangesAsync();

            StubSuccessfulConversion();
            _logoStorage
                .SaveAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new StoredTenantLogo("Tenant/acme/Photos/Logo/new.webp", "https://api.photobiz.test/media/Tenant/acme/Photos/Logo/new.webp"));

            await _handler.Handle(Command(), CancellationToken.None);

            await _logoStorage.Received(1).DeleteAsync("Tenant/acme/Photos/Logo/old.webp", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenNoPreviousLogoExists_NeverCallsDelete()
        {
            await SeedTenantAsync();
            StubSuccessfulConversion();
            _logoStorage
                .SaveAsync(Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>())
                .Returns(new StoredTenantLogo("Tenant/acme/Photos/Logo/new.webp", "https://api.photobiz.test/media/Tenant/acme/Photos/Logo/new.webp"));

            await _handler.Handle(Command(), CancellationToken.None);

            await _logoStorage.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WhenTheImageProcessorRejectsTheFile_ThrowsValidationException()
        {
            await SeedTenantAsync();
            _imageProcessor
                .ConvertToWebPAsync(Arg.Any<Stream>(), Arg.Any<int>(), Arg.Any<CancellationToken>())
                .Returns<Task<ProcessedImage>>(_ => throw new UnsupportedImageException("bad image", new InvalidDataException()));

            await Assert.ThrowsAsync<ValidationException>(() => _handler.Handle(Command(), CancellationToken.None));

            await _logoStorage.DidNotReceive().SaveAsync(
                Arg.Any<string>(), Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WithNoCurrentTenantKey_ThrowsTenantNotFoundException()
        {
            _tenantService.GetCurrentTenantKey().Returns((string?)null);

            await Assert.ThrowsAsync<TenantNotFoundException>(() =>
                _handler.Handle(Command(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithUnknownTenantKey_ThrowsTenantNotFoundException()
        {
            _tenantService.GetCurrentTenantKey().Returns("ghost");

            await Assert.ThrowsAsync<TenantNotFoundException>(() =>
                _handler.Handle(Command(), CancellationToken.None));
        }
    }
}
