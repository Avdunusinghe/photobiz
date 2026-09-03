namespace Photobiz.Domain.Entities
{
    public class SessionType
    {
        public Guid Id { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        public decimal Price { get; set; }

        public int DurationMinutes { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; } = [];
    }
}
