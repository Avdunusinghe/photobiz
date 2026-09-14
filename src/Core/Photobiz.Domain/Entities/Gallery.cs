using Photobiz.Domain.Common;
using Photobiz.Domain.Enums;

namespace Photobiz.Domain.Entities
{
    public class Gallery : AuditableEntity<Guid>
    {
        public required string Title { get; set; }

        public string? Description { get; set; }

        public Guid UserId { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual ICollection<Photo> Photos { get; set; } = [];

        /// <summary>Null falls back to the tenant's <see cref="SiteTheme.DefaultGalleryTemplate"/>.</summary>
        public GalleryTemplate? Template { get; set; }
    }
}
