namespace Photobiz.Domain.Entities
{
    public class Role
    {
        public Guid Id { get; set; }

        public required string Name { get; set; }

        public virtual ICollection<UserRole> UserRoles { get; set; } = [];
    }
}
