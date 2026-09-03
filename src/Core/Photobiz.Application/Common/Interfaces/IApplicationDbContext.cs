using Microsoft.EntityFrameworkCore;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Common.Interfaces
{
    public interface IApplicationDbContext
    {
        DbSet<User> Users { get; }

        DbSet<Role> Roles { get; }

        DbSet<UserRole> UserRoles { get; }

        DbSet<Client> Clients { get; }

        DbSet<Gallery> Galleries { get; }

        DbSet<Photo> Photos { get; }

        DbSet<SessionType> SessionTypes { get; }

        DbSet<Booking> Bookings { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
