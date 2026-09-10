namespace Photobiz.Domain.Common
{
    /// <summary>
    /// Base class for entities whose creation and last modification are tracked.
    /// The four fields are populated automatically by <c>AuditableEntityInterceptor</c>
    /// during <c>SaveChanges</c>; application code never needs to set them.
    /// </summary>
    public abstract class AuditableEntity
    {
        public DateTime CreatedAt { get; set; }

        public string? CreatedBy { get; set; }

        public DateTime? UpdatedAt { get; set; }

        public string? UpdatedBy { get; set; }
    }
}
