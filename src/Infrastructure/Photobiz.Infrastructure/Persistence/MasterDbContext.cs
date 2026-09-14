using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Domain.Common;
using Photobiz.Domain.Entities.Master;

namespace Photobiz.Infrastructure.Persistence
{
    /// <summary>
    /// The tenant registry. Holds only <see cref="Tenant"/> rows — one per tenant database — and
    /// nothing else; business data (Users, Bookings, ...) lives exclusively in
    /// <see cref="PhotobizDbContext"/>, one instance of that schema per tenant database.
    /// </summary>
    public class MasterDbContext : DbContext, IMasterDbContext
    {
        public MasterDbContext(DbContextOptions<MasterDbContext> options) : base(options)
        {
        }

        public DbSet<Tenant> Tenants => Set<Tenant>();

        public DbSet<SmtpSetting> SmtpSettings => Set<SmtpSetting>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Only the ".Configurations.Master" namespace applies here — see the mirrored
            // exclusion in PhotobizDbContext.OnModelCreating for why the split exists.
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(MasterDbContext).Assembly,
                type => type.Namespace is not null && type.Namespace.Contains(".Configurations.Master"));

            foreach (var entityType in modelBuilder.Model.GetEntityTypes()
                .Where(type => typeof(AuditableEntity).IsAssignableFrom(type.ClrType)))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditableEntity.CreatedBy))
                    .HasMaxLength(256);

                modelBuilder.Entity(entityType.ClrType)
                    .Property(nameof(AuditableEntity.UpdatedBy))
                    .HasMaxLength(256);
            }
        }
    }
}
