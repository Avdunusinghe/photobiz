using Photobiz.Domain.Entities;

namespace Photobiz.Application.Features.SiteThemes.Common
{
    /// <summary>The starting theme a tenant gets before they've customized anything, shared by the seeder and the get-or-create handlers.</summary>
    public static class SiteThemeDefaults
    {
        public static SiteTheme Create() => new()
        {
            Id = Guid.NewGuid(),
            PrimaryColor = "#111827",
            SecondaryColor = "#F97316",
            AccentColor = "#2563EB",
        };
    }
}
