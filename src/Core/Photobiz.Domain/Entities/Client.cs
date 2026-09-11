using Photobiz.Domain.Common;

namespace Photobiz.Domain.Entities
{
    public class Client : AuditableEntity<Guid>
    {
        public required string Name { get; set; }

        public required string Email { get; set; }

        public string? Phone { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; } = [];
    }
}
