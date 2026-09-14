using Photobiz.Domain.Common;
using Photobiz.Domain.Enums;

namespace Photobiz.Domain.Entities
{
    /// <summary>A social/contact link shown in the tenant's public portfolio site footer.</summary>
    public class SiteFooterLink : AuditableEntity<Guid>
    {
        public Guid SiteThemeId { get; set; }

        public virtual SiteTheme SiteTheme { get; set; } = null!;

        public FooterLinkPlatform Platform { get; set; }

        public required string Url { get; set; }

        public int DisplayOrder { get; set; }

        public bool IsActive { get; set; } = true;
    }
}
