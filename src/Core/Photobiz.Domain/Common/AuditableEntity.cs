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

    /// <summary>
    /// <see cref="AuditableEntity"/> that also owns its primary key, so entities only need to
    /// pick a key type (<c>AuditableEntity&lt;Guid&gt;</c>, <c>AuditableEntity&lt;int&gt;</c>, ...)
    /// instead of re-declaring <c>Id</c> themselves.
    /// </summary>
    public abstract class AuditableEntity<TId> : AuditableEntity
    {
        public TId Id { get; set; } = default!;
    }
}
