using NSubstitute;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Features.Tenants.GetTenantDetails;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Entities.Master;

namespace Photobiz.Application.Tests.Features.Tenants.GetTenantDetails
{
    public class GetTenantDetailsQueryHandlerTests
    {
        private readonly InMemoryMasterDbContext _dbContext = InMemoryMasterDbContext.Create();
        private readonly ITenantService _tenantService = Substitute.For<ITenantService>();
        private readonly GetTenantDetailsQueryHandler _handler;

        public GetTenantDetailsQueryHandlerTests()
        {
            _handler = new GetTenantDetailsQueryHandler(_dbContext, _tenantService, TestMappingConfig.Create());
        }

        private static Tenant NewTenant(string tenantKey = "acme") => Tenant.Create(
            tenantKey, "Acme Studio", "Server=.;Database=Tenant_Acme;",
            "owner@acme.test", "Ada", "Lovelace", "+1 555 0100", "1 Street", "London", "UK",
            "pro", "monthly", Guid.NewGuid());

        [Fact]
        public async Task Handle_ReturnsTheCurrentTenantsProfile()
        {
            var tenant = NewTenant();
            tenant.UpdateProfile(
                "Acme Studio", "owner@acme.test",
                "Ada", "Lovelace", "+1 555 0100", "1 Street", "London", "UK");
            tenant.SetLogo("https://cdn.acmestudio.test/logo.webp", "Tenant/acme/Photos/Logo/acme-logo-abc.webp");
            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync();
            _tenantService.GetCurrentTenantKey().Returns("acme");

            var result = await _handler.Handle(new GetTenantDetailsQuery(), CancellationToken.None);

            Assert.Equal("acme", result.TenantKey);
            Assert.Equal("Acme Studio", result.Name);
            Assert.Equal("https://cdn.acmestudio.test/logo.webp", result.LogoUrl);
            Assert.Equal("owner@acme.test", result.CustomerEmail);
        }

        [Fact]
        public async Task Handle_ReflectsSubscriptionAndCustomDomainStatus()
        {
            var tenant = NewTenant();
            tenant.RecordSubscriptionPayment("pay_123", "sub_456", DateOnly.FromDateTime(DateTime.UtcNow));
            tenant.RequestCustomDomain("www.acmestudio.test");
            tenant.MarkCustomDomainVerified();
            _dbContext.Tenants.Add(tenant);
            await _dbContext.SaveChangesAsync();
            _tenantService.GetCurrentTenantKey().Returns("acme");

            var result = await _handler.Handle(new GetTenantDetailsQuery(), CancellationToken.None);

            Assert.True(result.IsSubscribed);
            Assert.Equal("www.acmestudio.test", result.CustomDomain);
            Assert.NotNull(result.CustomDomainVerifiedAt);
        }

        [Fact]
        public async Task Handle_WithNoCurrentTenantKey_ThrowsTenantNotFoundException()
        {
            _tenantService.GetCurrentTenantKey().Returns((string?)null);

            await Assert.ThrowsAsync<TenantNotFoundException>(() =>
                _handler.Handle(new GetTenantDetailsQuery(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithUnknownTenantKey_ThrowsTenantNotFoundException()
        {
            _tenantService.GetCurrentTenantKey().Returns("ghost");

            await Assert.ThrowsAsync<TenantNotFoundException>(() =>
                _handler.Handle(new GetTenantDetailsQuery(), CancellationToken.None));
        }
    }
}
