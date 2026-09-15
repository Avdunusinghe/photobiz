using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Photobiz.Domain.Entities;
using Photobiz.Infrastructure.Persistence;
using Photobiz.Infrastructure.Persistence.Seeding;

namespace Photobiz.Infrastructure.Tests.Persistence.Seeding
{
    public class GallerySeederTests
    {
        private static PhotobizDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<PhotobizDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static async Task<User> SeedUserAsync(PhotobizDbContext dbContext)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = "admin",
                PasswordHash = "hash",
                FirstName = "System",
                LastName = "Administrator",
                Email = "admin@photobiz.local",
                IsActive = true,
            };
            dbContext.Users.Add(user);
            await dbContext.SaveChangesAsync();
            return user;
        }

        [Fact]
        public async Task SeedAsync_WhenNoGalleriesExist_CreatesDemoGalleriesWithPhotos()
        {
            await using var dbContext = CreateContext();
            await SeedUserAsync(dbContext);

            await GallerySeeder.SeedAsync(dbContext, NullLogger.Instance);

            var galleries = await dbContext.Galleries.Include(g => g.Photos).ToListAsync();
            Assert.NotEmpty(galleries);
            Assert.All(galleries, g => Assert.NotEmpty(g.Photos));
        }

        [Fact]
        public async Task SeedAsync_CalledTwice_DoesNotDuplicateGalleries()
        {
            await using var dbContext = CreateContext();
            await SeedUserAsync(dbContext);

            await GallerySeeder.SeedAsync(dbContext, NullLogger.Instance);
            var countAfterFirst = await dbContext.Galleries.CountAsync();

            await GallerySeeder.SeedAsync(dbContext, NullLogger.Instance);
            var countAfterSecond = await dbContext.Galleries.CountAsync();

            Assert.Equal(countAfterFirst, countAfterSecond);
        }

        [Fact]
        public async Task SeedAsync_WhenNoUserExistsYet_DoesNothing()
        {
            await using var dbContext = CreateContext();

            await GallerySeeder.SeedAsync(dbContext, NullLogger.Instance);

            Assert.Empty(await dbContext.Galleries.ToListAsync());
        }
    }
}
