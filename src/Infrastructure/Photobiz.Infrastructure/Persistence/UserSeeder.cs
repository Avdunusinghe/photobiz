using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Photobiz.Domain.Entities;

namespace Photobiz.Infrastructure.Persistence
{
    public static class UserSeeder
    {
        private const string DefaultUsername = "admin";
        private const string DefaultPassword = "Photobiz!2026";

        public static async Task SeedAsync(
            PhotobizDbContext dbContext,
            IPasswordHasher<User> passwordHasher,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            var roles = await RoleSeeder.SeedAsync(dbContext, cancellationToken);

            if (await dbContext.Users.AnyAsync(cancellationToken))
            {
                return;
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = DefaultUsername,
                PasswordHash = string.Empty,
                CreatedAt = DateTime.UtcNow,
            };
            user.PasswordHash = passwordHasher.HashPassword(user, DefaultPassword);

            dbContext.Users.Add(user);
            dbContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roles[RoleNames.Admin].Id });

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded initial {Role} user {Username}.", RoleNames.Admin, DefaultUsername);
        }
    }
}
