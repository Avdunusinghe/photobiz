using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Photobiz.Domain.Entities.Master;

namespace Photobiz.Infrastructure.Persistence.Configurations.Master
{
    /// <summary>
    /// Applied only to <see cref="MasterDbContext"/> — see the namespace-based split in
    /// <c>PhotobizDbContext.OnModelCreating</c> / <c>MasterDbContext.OnModelCreating</c>.
    /// </summary>
    public class SmtpSettingConfiguration : IEntityTypeConfiguration<SmtpSetting>
    {
        public void Configure(EntityTypeBuilder<SmtpSetting> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Host)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.Password)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(x => x.FromEmail)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.FromName)
                .HasMaxLength(256);

            // One SMTP configuration per tenant; EF derives a unique index on TenantId from this
            // one-to-one relationship automatically.
            builder.HasOne(x => x.Tenant)
                .WithOne(x => x.SmtpSetting)
                .HasForeignKey<SmtpSetting>(x => x.TenantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
