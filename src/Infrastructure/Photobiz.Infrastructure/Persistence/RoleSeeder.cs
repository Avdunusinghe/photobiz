using Microsoft.EntityFrameworkCore;
using Photobiz.Domain.Entities;

namespace Photobiz.Infrastructure.Persistence
{
    public static class RoleSeeder
    {
        public static async Task<IReadOnlyDictionary<string, Role>> SeedAsync(
            PhotobizDbContext dbContext,
            CancellationToken cancellationToken = default)
        {
            var roles = await dbContext.Roles.ToDictionaryAsync(x => x.Name, cancellationToken);

            foreach (var roleName in RoleNames.All)
            {
                if (roles.ContainsKey(roleName))
                {
                    continue;
                }

                var role = new Role { Id = Guid.NewGuid(), Name = roleName };
                dbContext.Roles.Add(role);
                roles[roleName] = role;
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            return roles;
        }
    }
}
