using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Features.Tenants.UpdateTenantDetails;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Entities.Master;

namespace Photobiz.Application.Tests.Features.Tenants.UpdateTenantDetails
{
    public class UpdateTenantDetailsCommandHandlerTests
    {
        private readonly InMemoryMasterDbContext _dbContext = InMemoryMasterDbContext.Create();
        private readonly ITenantService _tenantService = Substitute.For<ITenantService>();
        private readonly UpdateTenantDetailsCommandHandler _handler;

        public UpdateTenantDetailsCommandHandlerTests()
        {
            _handler = new UpdateTenantDetailsCommandHandler(_dbContext, _tenantService, TestMappingConfig.Create());
        }

        private static UpdateTenantDetailsCommand Command(
            string name = "Acme Studio Ltd",
            string customerEmail = "hello@acmestudio.test",
            string customerFirstName = "Grace",
            string customerLastName = "Hopper",
            string phone = "+1 555 0199",
            string address = "221B Baker Street",
            string city = "Manchester",
            string country = "UK") =>
            new(name, customerEmail, customerFirstName, customerLastName, phone, address, city, country);

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
        public async Task Handle_UpdatesTheCurrentTenantsProfile()
        {
            var tenant = await SeedTenantAsync();

            var result = await _handler.Handle(Command(), CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal("Tenant details updated successfully.", result.Message);
            Assert.Equal("Acme Studio Ltd", result.Data!.Name);
            Assert.Equal("hello@acmestudio.test", result.Data.CustomerEmail);

            var stored = await _dbContext.Tenants.SingleAsync(t => t.Id == tenant.Id);
            Assert.Equal("Acme Studio Ltd", stored.Name);
        }

        [Fact]
        public async Task Handle_NeverTouchesTheLogo()
        {
            var tenant = await SeedTenantAsync();
            tenant.SetLogo("https://cdn.acmestudio.test/logo.webp", "Tenant/acme/Photos/Logo/a.webp");
            await _dbContext.SaveChangesAsync();

            var result = await _handler.Handle(Command(), CancellationToken.None);

            Assert.Equal("https://cdn.acmestudio.test/logo.webp", result.Data!.LogoUrl);
        }

        [Fact]
        public async Task Handle_DoesNotTouchConnectionStringOrSubscriptionState()
        {
            var tenant = await SeedTenantAsync();
            tenant.RecordSubscriptionPayment("pay_123", "sub_456", DateOnly.FromDateTime(DateTime.UtcNow));
            await _dbContext.SaveChangesAsync();

            await _handler.Handle(Command(), CancellationToken.None);

            var stored = await _dbContext.Tenants.SingleAsync(t => t.Id == tenant.Id);
            Assert.Equal("Server=.;Database=Tenant_Acme;", stored.ConnectionString);
            Assert.True(stored.IsSubscribed);
            Assert.Equal("pay_123", stored.PayHerePaymentId);
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
