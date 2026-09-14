using NSubstitute;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Features.SmtpSettings.GetSmtpSettings;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Entities.Master;

namespace Photobiz.Application.Tests.Features.SmtpSettings.GetSmtpSettings
{
    public class GetSmtpSettingsQueryHandlerTests
    {
        private readonly InMemoryMasterDbContext _dbContext = InMemoryMasterDbContext.Create();
        private readonly ITenantService _tenantService = Substitute.For<ITenantService>();
        private readonly GetSmtpSettingsQueryHandler _handler;

        public GetSmtpSettingsQueryHandlerTests()
        {
            _handler = new GetSmtpSettingsQueryHandler(_dbContext, _tenantService, TestMappingConfig.Create());
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
        public async Task Handle_WhenConfigured_ReturnsTheSettingsWithoutThePassword()
        {
            var tenant = await SeedTenantAsync();
            var smtp = SmtpSetting.Create(
                tenant.Id, "smtp.acmestudio.test", 587,
                "no-reply@acmestudio.test", "s3cret", true,
                "no-reply@acmestudio.test", "Acme Studio");
            _dbContext.SmtpSettings.Add(smtp);
            await _dbContext.SaveChangesAsync();

            var result = await _handler.Handle(new GetSmtpSettingsQuery(), CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("smtp.acmestudio.test", result.Host);
            Assert.Equal(587, result.Port);
            Assert.Equal("no-reply@acmestudio.test", result.Username);
            Assert.True(result.EnableSsl);
            Assert.Equal("no-reply@acmestudio.test", result.FromEmail);
            Assert.Equal("Acme Studio", result.FromName);
            Assert.True(result.IsEnabled);
            Assert.DoesNotContain("s3cret", result.ToString());
        }

        [Fact]
        public async Task Handle_WhenNotConfigured_ReturnsNull()
        {
            await SeedTenantAsync();

            var result = await _handler.Handle(new GetSmtpSettingsQuery(), CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task Handle_ReflectsWhenTheSettingIsDisabled()
        {
            var tenant = await SeedTenantAsync();
            var smtp = SmtpSetting.Create(
                tenant.Id, "smtp.acmestudio.test", 587,
                "no-reply@acmestudio.test", "s3cret", true,
                "no-reply@acmestudio.test", null);
            smtp.Disable();
            _dbContext.SmtpSettings.Add(smtp);
            await _dbContext.SaveChangesAsync();

            var result = await _handler.Handle(new GetSmtpSettingsQuery(), CancellationToken.None);

            Assert.NotNull(result);
            Assert.False(result.IsEnabled);
            Assert.Null(result.FromName);
        }

        [Fact]
        public async Task Handle_WithNoCurrentTenantKey_ThrowsTenantNotFoundException()
        {
            _tenantService.GetCurrentTenantKey().Returns((string?)null);

            await Assert.ThrowsAsync<TenantNotFoundException>(() =>
                _handler.Handle(new GetSmtpSettingsQuery(), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithUnknownTenantKey_ThrowsTenantNotFoundException()
        {
            _tenantService.GetCurrentTenantKey().Returns("ghost");

            await Assert.ThrowsAsync<TenantNotFoundException>(() =>
                _handler.Handle(new GetSmtpSettingsQuery(), CancellationToken.None));
        }
    }
}
