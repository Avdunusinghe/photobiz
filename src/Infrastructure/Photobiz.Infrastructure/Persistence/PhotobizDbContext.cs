using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Interfaces;
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
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(PhotobizDbContext).Assembly);
        }
    }
}
