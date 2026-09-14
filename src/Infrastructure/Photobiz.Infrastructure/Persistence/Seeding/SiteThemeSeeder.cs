using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Photobiz.Domain.Entities;
using Photobiz.Domain.Enums;
using Photobiz.Infrastructure.Persistence;

namespace Photobiz.Infrastructure.Persistence.Seeding
{
    /// <summary>
    /// Gives the tenant a default portfolio site theme on first run, so the public site and the
    /// "Website Theme" admin screen have something to show without a manual DB insert. There is
    /// exactly one <see cref="SiteTheme"/> row per tenant database — no TenantId needed.
    /// </summary>
    public static class SiteThemeSeeder
    {
        public static async Task SeedAsync(
            PhotobizDbContext dbContext,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            if (await dbContext.SiteThemes.AnyAsync(cancellationToken))
            {
                return;
            }

            var theme = new SiteTheme
            {
                Id = Guid.NewGuid(),
                PrimaryColor = "#111827",
                SecondaryColor = "#F97316",
                AccentColor = "#2563EB",
                GradientDirection = GradientDirection.ToRight,
                HeaderStyle = HeaderStyle.Classic,
                DefaultGalleryTemplate = GalleryTemplate.Grid,
            };

            dbContext.SiteThemes.Add(theme);
            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded default portfolio site theme.");
        }
    }
}
