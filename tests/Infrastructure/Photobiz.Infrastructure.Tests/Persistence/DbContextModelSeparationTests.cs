using Microsoft.EntityFrameworkCore;
using Photobiz.Domain.Entities;
using Photobiz.Infrastructure.Persistence;

namespace Photobiz.Infrastructure.Tests.Persistence
{
    /// <summary>
    /// PhotobizDbContext and MasterDbContext both call ApplyConfigurationsFromAssembly over the
    /// SAME assembly, filtered by namespace (see the comments in each OnModelCreating). These
    /// tests guard that split: it's easy to silently break by adding a new configuration in the
    /// wrong folder, which would leak Tenant into every tenant database, or a tenant-schema entity
    /// into the Master database.
    /// </summary>
    public class DbContextModelSeparationTests
    {
        private static PhotobizDbContext CreateTenantContext() =>
            new(new DbContextOptionsBuilder<PhotobizDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static MasterDbContext CreateMasterContext() =>
            new(new DbContextOptionsBuilder<MasterDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        [Fact]
        public void TenantDbContext_DoesNotIncludeTheTenantEntity()
        {
            using var dbContext = CreateTenantContext();

            Assert.Null(dbContext.Model.FindEntityType(typeof(Tenant)));
        }

        [Fact]
        public void TenantDbContext_IncludesEveryExpectedTenantSchemaEntity()
        {
            using var dbContext = CreateTenantContext();

            Type[] expected =
            [
                typeof(User), typeof(Role), typeof(UserRole),
                typeof(Client), typeof(Gallery), typeof(Photo),
                typeof(SessionType), typeof(Booking)
            ];

            Assert.All(expected, type => Assert.NotNull(dbContext.Model.FindEntityType(type)));
        }

        [Fact]
        public void MasterDbContext_OnlyIncludesTheTenantEntity()
        {
            using var dbContext = CreateMasterContext();

            var entityTypes = dbContext.Model.GetEntityTypes().Select(t => t.ClrType).ToList();

            Assert.Equal([typeof(Tenant)], entityTypes);
        }
    }
}
