using Microsoft.EntityFrameworkCore;
using Photobiz.Infrastructure.Persistence;

namespace Photobiz.Api.Extensions
{
    public static class DatabaseMigrationExtensions
    {
        public static async Task MigrateDatabaseAsync(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                return;
            }

            using var scope = app.Services.CreateScope();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigration");

            // Master first: TenantSeeder (run right after this) needs it in place before it can
            // register the dev tenant, and the tenant DbContext below is still pointed at its
            // configured default connection string at this point (no request/middleware yet).
            await MigrateAsync(scope.ServiceProvider.GetRequiredService<MasterDbContext>(), "Master", logger);
            await MigrateAsync(scope.ServiceProvider.GetRequiredService<PhotobizDbContext>(), "Tenant (default)", logger);
        }

        private static async Task MigrateAsync(DbContext dbContext, string label, ILogger logger)
        {
            var databaseExists = await dbContext.Database.CanConnectAsync();
            logger.LogInformation(
                databaseExists
                    ? "{Label} database already exists."
                    : "{Label} database does not exist yet; it will be created.",
                label);

            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

            if (pendingMigrations.Count == 0)
            {
                logger.LogInformation("{Label} database is up to date, no migrations to apply.", label);
                return;
            }

            logger.LogInformation(
                "Applying {Count} pending {Label} migration(s): {Migrations}",
                pendingMigrations.Count,
                label,
                string.Join(", ", pendingMigrations));

            // Creates the database if it doesn't exist yet, then applies every pending migration.
            await dbContext.Database.MigrateAsync();
        }
    }
}
