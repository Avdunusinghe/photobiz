using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Photobiz.Domain.Entities;
using Photobiz.Infrastructure.Persistence;

namespace Photobiz.Infrastructure.Persistence.Seeding.Master
{
    /// <summary>
    /// Registers the local dev tenant in the Master database on first run, pointing it at the
    /// existing tenant connection string so `dotnet run` keeps working end to end without a real
    /// tenant-provisioning workflow. Real tenants are created via the (future) provisioning
    /// command, not this seeder.
    /// </summary>
    public static class TenantSeeder
    {
        public const string DefaultTenantKey = "default";

        public static async Task SeedAsync(
            MasterDbContext masterDbContext,
            string tenantConnectionString,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            var exists = await masterDbContext.Tenants
                .AnyAsync(t => t.TenantKey == DefaultTenantKey, cancellationToken);

            if (exists)
            {
                return;
            }

            var tenant = Tenant.Create(
                tenantKey: DefaultTenantKey,
                name: "Default Tenant",
                connectionString: tenantConnectionString,
                customerEmail: "admin@photobiz.local",
                customerFirstName: "System",
                customerLastName: "Administrator",
                phone: "N/A",
                address: "N/A",
                city: "N/A",
                country: "N/A",
                planCode: "dev",
                billingCycle: "n/a",
                orderId: Guid.Empty);

            masterDbContext.Tenants.Add(tenant);
            await masterDbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded default tenant '{TenantKey}'.", DefaultTenantKey);
        }
    }
}
