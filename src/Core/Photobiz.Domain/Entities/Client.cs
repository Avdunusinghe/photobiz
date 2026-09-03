namespace Photobiz.Domain.Entities
{
    public class Client
    {
        public Guid Id { get; set; }

        public required string Name { get; set; }

        public required string Email { get; set; }

        public string? Phone { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<Booking> Bookings { get; set; } = [];
    }
}
