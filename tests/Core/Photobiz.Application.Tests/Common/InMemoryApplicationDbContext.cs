using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Tests.Common
{
    /// <summary>
    /// EF Core in-memory <see cref="IApplicationDbContext"/> used by Application feature tests.
    /// </summary>
    public sealed class InMemoryApplicationDbContext : DbContext, IApplicationDbContext
    {
        public InMemoryApplicationDbContext(DbContextOptions<InMemoryApplicationDbContext> options)
            : base(options)
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

        public static InMemoryApplicationDbContext Create()
        {
            var options = new DbContextOptionsBuilder<InMemoryApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new InMemoryApplicationDbContext(options);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<UserRole>().HasKey(x => new { x.UserId, x.RoleId });

            // Mirror the production soft-delete filter from UserConfiguration.
            modelBuilder.Entity<User>().HasQueryFilter(x => x.IsActive);
        }
    }
}
