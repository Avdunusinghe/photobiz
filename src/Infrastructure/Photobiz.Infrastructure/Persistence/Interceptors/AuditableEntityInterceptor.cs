using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Domain.Common;

namespace Photobiz.Infrastructure.Persistence.Interceptors
{
    /// <summary>
    /// Populates <see cref="AuditableEntity"/> audit fields on every <c>SaveChanges</c>:
    /// <c>CreatedAt</c> / <c>CreatedBy</c> on insert, <c>UpdatedAt</c> / <c>UpdatedBy</c> on update.
    /// </summary>
    public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
    {
        private readonly ICurrentUser _currentUser;

        public AuditableEntityInterceptor(ICurrentUser currentUser)
        {
            _currentUser = currentUser;
        }

        public override InterceptionResult<int> SavingChanges(
            DbContextEventData eventData,
            InterceptionResult<int> result)
        {
            ApplyAudit(eventData.Context);
            return base.SavingChanges(eventData, result);
        }

        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
            DbContextEventData eventData,
            InterceptionResult<int> result,
            CancellationToken cancellationToken = default)
        {
            ApplyAudit(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        public void ApplyAudit(DbContext? context)
        {
            if (context is null)
            {
                return;
            }

            var timestamp = DateTime.UtcNow;
            var user = _currentUser.UserName;

            foreach (var entry in context.ChangeTracker.Entries<AuditableEntity>())
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Entity.CreatedAt = timestamp;
                        entry.Entity.CreatedBy = user;
                        break;

                    case EntityState.Modified:
                        entry.Entity.UpdatedAt = timestamp;
                        entry.Entity.UpdatedBy = user;

                        // The creation stamp is immutable once written.
                        entry.Property(e => e.CreatedAt).IsModified = false;
                        entry.Property(e => e.CreatedBy).IsModified = false;
                        break;
                }
            }
        }
    }
}
