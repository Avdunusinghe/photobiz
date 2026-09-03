namespace Photobiz.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public required string Username { get; set; }

        public required string PasswordHash { get; set; }

        public DateTime CreatedAt { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = [];

        public virtual ICollection<Gallery> Galleries { get; set; } = [];
    }
}
