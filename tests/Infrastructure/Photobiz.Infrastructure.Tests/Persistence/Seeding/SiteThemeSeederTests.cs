using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Photobiz.Infrastructure.Persistence;
using Photobiz.Infrastructure.Persistence.Seeding;

namespace Photobiz.Infrastructure.Tests.Persistence.Seeding
{
    public class SiteThemeSeederTests
    {
        private static PhotobizDbContext CreateContext() =>
            new(new DbContextOptionsBuilder<PhotobizDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        [Fact]
        public async Task SeedAsync_WhenNoThemeExists_CreatesADefaultTheme()
        {
            await using var dbContext = CreateContext();

            await SiteThemeSeeder.SeedAsync(dbContext, NullLogger.Instance);

            var theme = await dbContext.SiteThemes.SingleAsync();
            Assert.False(string.IsNullOrWhiteSpace(theme.PrimaryColor));
            Assert.False(string.IsNullOrWhiteSpace(theme.SecondaryColor));
            Assert.False(string.IsNullOrWhiteSpace(theme.AccentColor));
        }

        [Fact]
        public async Task SeedAsync_CalledTwice_DoesNotDuplicateTheTheme()
        {
            await using var dbContext = CreateContext();

            await SiteThemeSeeder.SeedAsync(dbContext, NullLogger.Instance);
            await SiteThemeSeeder.SeedAsync(dbContext, NullLogger.Instance);

            Assert.Equal(1, await dbContext.SiteThemes.CountAsync());
        }

        [Fact]
        public async Task SeedAsync_WhenAThemeAlreadyExists_DoesNotOverwriteIt()
        {
            await using var dbContext = CreateContext();
            dbContext.SiteThemes.Add(new Photobiz.Domain.Entities.SiteTheme
            {
                Id = Guid.NewGuid(),
                PrimaryColor = "#000000",
                SecondaryColor = "#111111",
                AccentColor = "#222222",
            });
            await dbContext.SaveChangesAsync();

            await SiteThemeSeeder.SeedAsync(dbContext, NullLogger.Instance);

            var theme = await dbContext.SiteThemes.SingleAsync();
            Assert.Equal("#000000", theme.PrimaryColor);
        }
    }
}
