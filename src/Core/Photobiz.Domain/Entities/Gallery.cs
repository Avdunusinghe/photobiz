using Photobiz.Domain.Common;

namespace Photobiz.Domain.Entities
{
    public class Gallery : AuditableEntity
    {
        public Guid Id { get; set; }

        public required string Title { get; set; }

        public string? Description { get; set; }

        public Guid UserId { get; set; }

        public virtual User User { get; set; } = null!;

        public virtual ICollection<Photo> Photos { get; set; } = [];
    }
}
