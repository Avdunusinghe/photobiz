using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Features.Tenants.RemoveTenantLogo;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Entities.Master;

namespace Photobiz.Application.Tests.Features.Tenants.RemoveTenantLogo
{
    public class RemoveTenantLogoCommandHandlerTests
    {
        private readonly InMemoryMasterDbContext _dbContext = InMemoryMasterDbContext.Create();
        private readonly ITenantService _tenantService = Substitute.For<ITenantService>();
        private readonly ITenantLogoStorage _logoStorage = Substitute.For<ITenantLogoStorage>();
        private readonly RemoveTenantLogoCommandHandler _handler;

        public RemoveTenantLogoCommandHandlerTests()
        {
            _handler = new RemoveTenantLogoCommandHandler(
                _dbContext, _tenantService, _logoStorage, TestMappingConfig.Create());
        }

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

        [Fact]
        public async Task Handle_WithAnExistingLogo_ClearsItAndDeletesTheFile()
        {
            var tenant = await SeedTenantAsync();
            tenant.SetLogo("https://api.photobiz.test/media/Tenant/acme/Photos/Logo/old.webp", "Tenant/acme/Photos/Logo/old.webp");
            await _dbContext.SaveChangesAsync();

            var result = await _handler.Handle(new RemoveTenantLogoCommand(), CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal("Logo removed successfully.", result.Message);
            Assert.Null(result.Data!.LogoUrl);

            var stored = await _dbContext.Tenants.SingleAsync(t => t.Id == tenant.Id);
            Assert.Null(stored.LogoUrl);
            Assert.Null(stored.LogoStoragePath);

            await _logoStorage.Received(1).DeleteAsync("Tenant/acme/Photos/Logo/old.webp", Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WithNoExistingLogo_IsANoOpAndNeverCallsDelete()
        {
            await SeedTenantAsync();

            var result = await _handler.Handle(new RemoveTenantLogoCommand(), CancellationToken.None);

            Assert.True(result.Success);
            Assert.Null(result.Data!.LogoUrl);

            await _logoStorage.DidNotReceive().DeleteAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task Handle_WithNoCurrentTenantKey_ThrowsTenantNotFoundException()
        {
            _tenantService.GetCurrentTenantKey().Returns((string?)null);

            await Assert.ThrowsAsync<TenantNotFoundException>(() =>
                _handler.Handle(new RemoveTenantLogoCommand(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithUnknownTenantKey_ThrowsTenantNotFoundException()
        {
            _tenantService.GetCurrentTenantKey().Returns("ghost");

            await Assert.ThrowsAsync<TenantNotFoundException>(() =>
                _handler.Handle(new RemoveTenantLogoCommand(), CancellationToken.None));
        }
    }
}
