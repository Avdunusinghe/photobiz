using Photobiz.Domain.Common;
using Photobiz.Domain.Enums;

namespace Photobiz.Domain.Entities
{
    /// <summary>
    /// The tenant's public portfolio site appearance — colors, gradient, header/footer style, and
    /// default gallery layout. A singleton within the tenant database: there is exactly one row per
    /// tenant (no TenantId column needed — the tenant DB connection, resolved per-request by
    /// <c>TenantSelectionMiddleware</c>, already scopes everything in this database to one tenant).
    /// </summary>
    public class SiteTheme : AuditableEntity<Guid>
    {
        public required string PrimaryColor { get; set; }

        public required string SecondaryColor { get; set; }

        public required string AccentColor { get; set; }

        /// <summary>Null means a solid <see cref="PrimaryColor"/> background, no gradient.</summary>
        public string? GradientStartColor { get; set; }

        public string? GradientEndColor { get; set; }

        public GradientDirection GradientDirection { get; set; } = GradientDirection.ToRight;

        /// <summary>Null falls back to the portfolio site's platform default font.</summary>
        public string? FontFamily { get; set; }

        public HeaderStyle HeaderStyle { get; set; } = HeaderStyle.Classic;

        public string? Tagline { get; set; }

        public string? FooterText { get; set; }

        /// <summary>Null falls back to a computed "© {year} {tenant name}".</summary>
        public string? FooterCopyrightText { get; set; }

        public GalleryTemplate DefaultGalleryTemplate { get; set; } = GalleryTemplate.Grid;

        public virtual ICollection<SiteFooterLink> FooterLinks { get; set; } = [];
    }
}
