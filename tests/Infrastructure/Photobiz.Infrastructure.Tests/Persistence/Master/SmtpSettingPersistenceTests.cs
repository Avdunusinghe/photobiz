using Microsoft.EntityFrameworkCore;
using Photobiz.Domain.Entities.Master;
using Photobiz.Infrastructure.Persistence;

namespace Photobiz.Infrastructure.Tests.Persistence.Master
{
    public class SmtpSettingPersistenceTests
    {
        private static MasterDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<MasterDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static Tenant NewTenant(string tenantKey) => Tenant.Create(
            tenantKey, "Acme", $"Server=.;Database=Tenant_{tenantKey};",
            "owner@acme.test", "Ada", "Lovelace", "555", "1 Street", "London", "UK",
            "pro", "monthly", Guid.NewGuid());

        [Fact]
        public async Task SavingASmtpSetting_IsReachableThroughTheOwningTenant()
        {
            await using var dbContext = CreateContext();
            var tenant = NewTenant("acme");
            dbContext.Tenants.Add(tenant);
            await dbContext.SaveChangesAsync();

            var setting = SmtpSetting.Create(
                tenant.Id, "smtp.acmestudio.test", 587,
                "no-reply@acmestudio.test", "s3cret", true,
                "no-reply@acmestudio.test", "Acme Studio");
            dbContext.SmtpSettings.Add(setting);
            await dbContext.SaveChangesAsync();

            var reloaded = await dbContext.Tenants
                .Include(t => t.SmtpSetting)
                .SingleAsync(t => t.TenantKey == "acme");

            Assert.NotNull(reloaded.SmtpSetting);
            Assert.Equal("smtp.acmestudio.test", reloaded.SmtpSetting!.Host);
        }

        [Fact]
        public async Task EachTenantsSmtpSettingIsIsolatedFromOtherTenants()
        {
            await using var dbContext = CreateContext();
            var acme = NewTenant("acme");
            var globex = NewTenant("globex");
            dbContext.Tenants.AddRange(acme, globex);
            await dbContext.SaveChangesAsync();

            dbContext.SmtpSettings.Add(SmtpSetting.Create(
                acme.Id, "smtp.acme.test", 587, "acme-user", "pass", true, "no-reply@acme.test", null));
            dbContext.SmtpSettings.Add(SmtpSetting.Create(
                globex.Id, "smtp.globex.test", 25, "globex-user", "pass", false, "no-reply@globex.test", null));
            await dbContext.SaveChangesAsync();

            var acmeSetting = await dbContext.SmtpSettings.SingleAsync(s => s.TenantId == acme.Id);
            var globexSetting = await dbContext.SmtpSettings.SingleAsync(s => s.TenantId == globex.Id);

            Assert.Equal("smtp.acme.test", acmeSetting.Host);
            Assert.Equal("smtp.globex.test", globexSetting.Host);
        }

        [Fact]
        public async Task TenantWithNoSmtpSetting_HasNullNavigation()
        {
            await using var dbContext = CreateContext();
            var tenant = NewTenant("acme");
            dbContext.Tenants.Add(tenant);
            await dbContext.SaveChangesAsync();

            var reloaded = await dbContext.Tenants
                .Include(t => t.SmtpSetting)
                .SingleAsync(t => t.TenantKey == "acme");

            Assert.Null(reloaded.SmtpSetting);
        }
    }
}
