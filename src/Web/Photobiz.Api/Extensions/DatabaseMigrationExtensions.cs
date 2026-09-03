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
            var dbContext = scope.ServiceProvider.GetRequiredService<PhotobizDbContext>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseMigration");

            var databaseExists = await dbContext.Database.CanConnectAsync();
            logger.LogInformation(
                databaseExists
                    ? "Database already exists."
                    : "Database does not exist yet; it will be created.");

            var pendingMigrations = (await dbContext.Database.GetPendingMigrationsAsync()).ToList();

            if (pendingMigrations.Count == 0)
            {
                logger.LogInformation("Database is up to date, no migrations to apply.");
                return;
            }

            logger.LogInformation(
                "Applying {Count} pending migration(s): {Migrations}",
                pendingMigrations.Count,
                string.Join(", ", pendingMigrations));

            // Creates the database if it doesn't exist yet, then applies every pending migration.
            await dbContext.Database.MigrateAsync();
        }
    }
}
