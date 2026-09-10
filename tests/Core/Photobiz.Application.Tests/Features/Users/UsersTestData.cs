using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Tests.Features.Users
{
    internal static class UsersTestData
    {
        public static IReadOnlyDictionary<string, Role> SeedRoles(InMemoryApplicationDbContext dbContext)
        {
            var roles = RoleNames.All.ToDictionary(
                name => name,
                name => new Role { Id = Guid.NewGuid(), Name = name });

            dbContext.Roles.AddRange(roles.Values);
            dbContext.SaveChanges();

            return roles;
        }

        public static User AddUser(
            InMemoryApplicationDbContext dbContext,
            IReadOnlyDictionary<string, Role> roles,
            string username,
            params string[] roleNames)
            => AddUser(dbContext, roles, username, isActive: true, roleNames);

        public static User AddUser(
            InMemoryApplicationDbContext dbContext,
            IReadOnlyDictionary<string, Role> roles,
            string username,
            bool isActive,
            params string[] roleNames)
            => AddUserProfile(
                dbContext,
                roles,
                username,
                firstName: username,
                lastName: "Test",
                email: $"{username}@example.com",
                mobileNumber: null,
                isActive: isActive,
                roleNames);

        public static User AddUserProfile(
            InMemoryApplicationDbContext dbContext,
            IReadOnlyDictionary<string, Role> roles,
            string username,
            string firstName,
            string lastName,
            string email,
            string? mobileNumber,
            bool isActive,
            params string[] roleNames)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = "hash",
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                MobileNumber = mobileNumber,
                IsActive = isActive,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(user);

            foreach (var roleName in roleNames)
            {
                dbContext.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = roles[roleName].Id });
            }

            dbContext.SaveChanges();

            return user;
        }

        public static void SoftDelete(InMemoryApplicationDbContext dbContext, User user)
        {
            user.IsActive = false;
            dbContext.SaveChanges();
        }
    }
}
