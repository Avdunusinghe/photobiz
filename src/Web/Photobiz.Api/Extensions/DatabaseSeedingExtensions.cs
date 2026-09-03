using Microsoft.AspNetCore.Identity;
using Photobiz.Domain.Entities;
using Photobiz.Infrastructure.Persistence;

namespace Photobiz.Api.Extensions
{
    public static class DatabaseSeedingExtensions
    {
        public static async Task SeedDevelopmentDataAsync(this WebApplication app)
        {
            if (!app.Environment.IsDevelopment())
            {
                return;
            }

            using var scope = app.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<PhotobizDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeding");

            await UserSeeder.SeedAsync(dbContext, passwordHasher, logger);
        }
    }
}
