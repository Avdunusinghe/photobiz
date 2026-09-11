using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Domain.Common;
using Photobiz.Domain.Entities;

namespace Photobiz.Infrastructure.Persistence
{
    public class PhotobizDbContext : DbContext, IApplicationDbContext
    {
        public PhotobizDbContext(DbContextOptions<PhotobizDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users => Set<User>();

        public DbSet<Role> Roles => Set<Role>();

        public DbSet<UserRole> UserRoles => Set<UserRole>();

        public DbSet<Client> Clients => Set<Client>();

        public DbSet<Gallery> Galleries => Set<Gallery>();

        public DbSet<Photo> Photos => Set<Photo>();

        public DbSet<SessionType> SessionTypes => Set<SessionType>();

        public DbSet<Booking> Bookings => Set<Booking>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Photobiz.Infrastructure also hosts MasterDbContext's configurations (under the
            // ".Configurations.Master" namespace) in the same assembly. Excluding them here keeps
            // Master-only entities (Tenant) out of every tenant database's model.
            modelBuilder.ApplyConfigurationsFromAssembly(
                typeof(PhotobizDbContext).Assembly,
                type => type.Namespace is null || !type.Namespace.Contains(".Configurations.Master"));

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
