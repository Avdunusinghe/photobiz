using Microsoft.EntityFrameworkCore;
using Photobiz.Domain.Entities.Master;

namespace Photobiz.Application.Common.Interfaces
{
    /// <summary>
    /// The Master database, abstracted the same way <see cref="IApplicationDbContext"/> abstracts
    /// a tenant database — lets Application-layer handlers reach tenant-registry data
    /// (<see cref="Tenant"/>, <see cref="SmtpSetting"/>) without depending on Infrastructure.
    /// </summary>
    public interface IMasterDbContext
    {
        DbSet<Tenant> Tenants { get; }

        DbSet<SmtpSetting> SmtpSettings { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
