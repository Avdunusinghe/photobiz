using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Photobiz.Domain.Entities;

namespace Photobiz.Infrastructure.Persistence
{
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
            var dbContext = scope.ServiceProvider.GetRequiredService<PhotobizDbContext>();
            var passwordHasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
            var logger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("DatabaseSeeding");

            await UserSeeder.SeedAsync(dbContext, passwordHasher, logger, cancellationToken);
        }
    }
}
