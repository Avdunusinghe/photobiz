using Photobiz.Domain.Common;

namespace Photobiz.Domain.Entities
{
    public class Photo : AuditableEntity<Guid>
    {
        public required string ThumbnailUrl { get; set; }

        public required string MediumUrl { get; set; }

        public required string FullUrl { get; set; }

        public string? AltText { get; set; }

        public Guid GalleryId { get; set; }

        public virtual Gallery Gallery { get; set; } = null!;
    }
}
