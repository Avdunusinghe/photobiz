using Photobiz.Domain.Common;

namespace Photobiz.Domain.Entities
{
    public class User : AuditableEntity
    {
        public Guid Id { get; set; }

        public required string Username { get; set; }

        public required string PasswordHash { get; set; }

        public required string FirstName { get; set; }

        public required string LastName { get; set; }

        public required string Email { get; set; }

        public string? MobileNumber { get; set; }

        /// <summary>
        /// Doubles as the soft-delete flag: <c>false</c> hides the row from every query via the
        /// global filter in <c>UserConfiguration</c>. <c>DeleteUserCommand</c> clears it; the audit
        /// interceptor records who did it and when in <c>UpdatedBy</c> / <c>UpdatedAt</c>.
        /// </summary>
        public bool IsActive { get; set; } = true;

        public virtual ICollection<UserRole> UserRoles { get; set; } = [];

        public virtual ICollection<Gallery> Galleries { get; set; } = [];
    }
}
