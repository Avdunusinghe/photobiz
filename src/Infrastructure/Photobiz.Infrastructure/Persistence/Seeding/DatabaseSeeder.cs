using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Photobiz.Domain.Entities;
using Photobiz.Infrastructure.Persistence;
using Photobiz.Infrastructure.Persistence.Seeding.Master;

namespace Photobiz.Infrastructure.Persistence.Seeding
{
    /// <summary>
    /// Entry point that ties every individual seeder together. Master-database seeders live under
    /// <c>Seeding.Master</c>, tenant-database seeders directly under <c>Seeding</c> — mirroring the
    /// same split used for <c>Configurations</c> / <c>Configurations.Master</c>.
    /// </summary>
    public static class DatabaseSeeder
    {
        public static async Task SeedDevelopmentDataAsync(
            IServiceProvider serviceProvider,
            IHostEnvironment environment,
            CancellationToken cancellationToken = default)
        {
            if (!environment.IsDevelopment())
            {
                return;
            }

            using var scope = serviceProvider.CreateScope();
            var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
            var masterDbContext = scope.ServiceProvider.GetRequiredService<MasterDbContext>();
            var dbContext = scope.ServiceProvider.GetRequiredService<PhotobizDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeding");

            var tenantConnectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");

            await TenantSeeder.SeedAsync(masterDbContext, tenantConnectionString, logger, cancellationToken);
            await SmtpSettingSeeder.SeedAsync(masterDbContext, TenantSeeder.DefaultTenantKey, logger, cancellationToken);
            await UserSeeder.SeedAsync(dbContext, passwordHasher, logger, cancellationToken);
            await SiteThemeSeeder.SeedAsync(dbContext, logger, cancellationToken);
        }
    }
}
