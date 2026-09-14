using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Features.SiteThemes.Common
{
    /// <summary>
    /// Loads the tenant's one <see cref="SiteTheme"/> row (with its footer links, ordered), creating
    /// it with defaults on first access. There is no "theme not configured" state to show in the
    /// UI — every tenant always has a theme, self-healing here rather than requiring a provisioning
    /// step to have created one.
    /// </summary>
    internal static class SiteThemeAccessor
    {
        public static async Task<SiteTheme> GetOrCreateAsync(
            IApplicationDbContext dbContext,
            CancellationToken cancellationToken)
        {
            var theme = await dbContext.SiteThemes
                .Include(x => x.FooterLinks.OrderBy(link => link.DisplayOrder))
                .SingleOrDefaultAsync(cancellationToken);

            if (theme is not null)
            {
                return theme;
            }

            theme = SiteThemeDefaults.Create();
            dbContext.SiteThemes.Add(theme);
            await dbContext.SaveChangesAsync(cancellationToken);

            return theme;
        }
    }
}
