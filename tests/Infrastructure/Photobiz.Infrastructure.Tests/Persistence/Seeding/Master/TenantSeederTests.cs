using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Photobiz.Infrastructure.Persistence;
using Photobiz.Infrastructure.Persistence.Seeding.Master;

namespace Photobiz.Infrastructure.Tests.Persistence.Seeding.Master
{
    public class TenantSeederTests
    {
        private static MasterDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<MasterDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        [Fact]
        public async Task SeedAsync_CreatesTheDefaultTenantPointingAtTheGivenConnectionString()
        {
            await using var dbContext = CreateContext();

            await TenantSeeder.SeedAsync(dbContext, "Server=.;Database=PhotobizDb;", NullLogger.Instance);

            var tenant = await dbContext.Tenants.SingleAsync(t => t.TenantKey == TenantSeeder.DefaultTenantKey);
            Assert.Equal("Server=.;Database=PhotobizDb;", tenant.ConnectionString);
            Assert.True(tenant.IsSubscribed);
        }

        [Fact]
        public async Task SeedAsync_CalledTwice_DoesNotDuplicateTheDefaultTenant()
        {
            await using var dbContext = CreateContext();

            await TenantSeeder.SeedAsync(dbContext, "Server=.;Database=PhotobizDb;", NullLogger.Instance);
            await TenantSeeder.SeedAsync(dbContext, "Server=.;Database=PhotobizDb;", NullLogger.Instance);

            Assert.Equal(1, await dbContext.Tenants.CountAsync(t => t.TenantKey == TenantSeeder.DefaultTenantKey));
        }
    }
}
