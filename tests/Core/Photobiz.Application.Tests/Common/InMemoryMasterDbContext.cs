using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Domain.Entities.Master;

namespace Photobiz.Application.Tests.Common
{
    /// <summary>
    /// EF Core in-memory <see cref="IMasterDbContext"/> used by Application feature tests.
    /// </summary>
    public sealed class InMemoryMasterDbContext : DbContext, IMasterDbContext
    {
        public InMemoryMasterDbContext(DbContextOptions<InMemoryMasterDbContext> options)
            : base(options)
        {
        }

        public DbSet<Tenant> Tenants => Set<Tenant>();

        public DbSet<SmtpSetting> SmtpSettings => Set<SmtpSetting>();

        public static InMemoryMasterDbContext Create()
        {
            var options = new DbContextOptionsBuilder<InMemoryMasterDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;

            return new InMemoryMasterDbContext(options);
        }
    }
}
