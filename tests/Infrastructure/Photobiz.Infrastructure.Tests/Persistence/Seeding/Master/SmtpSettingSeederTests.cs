using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Photobiz.Domain.Entities.Master;
using Photobiz.Infrastructure.Persistence;
using Photobiz.Infrastructure.Persistence.Seeding.Master;

namespace Photobiz.Infrastructure.Tests.Persistence.Seeding.Master
{
    public class SmtpSettingSeederTests
    {
        private static MasterDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<MasterDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static async Task<Tenant> SeedTenantAsync(MasterDbContext dbContext, string tenantKey = "default")
        {
            var tenant = Tenant.Create(
                tenantKey, "Default Tenant", "Server=.;Database=PhotobizDb;",
                "admin@photobiz.local", "System", "Administrator", "N/A", "N/A", "N/A", "N/A",
                "dev", "n/a", Guid.Empty);
            dbContext.Tenants.Add(tenant);
            await dbContext.SaveChangesAsync();
            return tenant;
        }

        [Fact]
        public async Task SeedAsync_WhenTheTenantHasNoSmtpSettings_CreatesASampleRow()
        {
            await using var dbContext = CreateContext();
            var tenant = await SeedTenantAsync(dbContext);

            await SmtpSettingSeeder.SeedAsync(dbContext, tenant.TenantKey, NullLogger.Instance);

            var smtp = await dbContext.SmtpSettings.SingleAsync(s => s.TenantId == tenant.Id);
            Assert.False(string.IsNullOrWhiteSpace(smtp.Host));
            Assert.True(smtp.Port is > 0 and <= 65535);
            Assert.True(smtp.IsEnabled);
            Assert.Equal(tenant.Name, smtp.FromName);
        }

        [Fact]
        public async Task SeedAsync_CalledTwice_DoesNotDuplicateTheRow()
        {
            await using var dbContext = CreateContext();
            var tenant = await SeedTenantAsync(dbContext);

            await SmtpSettingSeeder.SeedAsync(dbContext, tenant.TenantKey, NullLogger.Instance);
            await SmtpSettingSeeder.SeedAsync(dbContext, tenant.TenantKey, NullLogger.Instance);

            Assert.Equal(1, await dbContext.SmtpSettings.CountAsync(s => s.TenantId == tenant.Id));
        }

        [Fact]
        public async Task SeedAsync_WhenTheTenantAlreadyHasSmtpSettings_DoesNotOverwriteThem()
        {
            await using var dbContext = CreateContext();
            var tenant = await SeedTenantAsync(dbContext);
            var existing = SmtpSetting.Create(
                tenant.Id, "smtp.acmestudio.test", 465, "existing-user", "existing-pass",
                true, "existing@acmestudio.test", "Existing Sender");
            dbContext.SmtpSettings.Add(existing);
            await dbContext.SaveChangesAsync();

            await SmtpSettingSeeder.SeedAsync(dbContext, tenant.TenantKey, NullLogger.Instance);

            var smtp = await dbContext.SmtpSettings.SingleAsync(s => s.TenantId == tenant.Id);
            Assert.Equal("smtp.acmestudio.test", smtp.Host);
        }

        [Fact]
        public async Task SeedAsync_WhenTheTenantDoesNotExist_DoesNothing()
        {
            await using var dbContext = CreateContext();

            await SmtpSettingSeeder.SeedAsync(dbContext, "ghost", NullLogger.Instance);

            Assert.Equal(0, await dbContext.SmtpSettings.CountAsync());
        }
    }
}
