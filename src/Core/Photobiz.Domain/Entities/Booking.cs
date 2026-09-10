using Photobiz.Domain.Common;
using Photobiz.Domain.Enums;

namespace Photobiz.Domain.Entities
{
    public class Booking : AuditableEntity
    {
        public Guid Id { get; set; }

        public DateTime SessionDate { get; set; }

        public BookingStatus Status { get; set; } = BookingStatus.Pending;

        public Guid ClientId { get; set; }

        public virtual Client Client { get; set; } = null!;

        public Guid SessionTypeId { get; set; }

        public virtual SessionType SessionType { get; set; } = null!;
    }
}
