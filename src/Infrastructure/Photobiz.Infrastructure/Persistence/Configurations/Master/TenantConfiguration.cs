using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Photobiz.Domain.Entities;

namespace Photobiz.Infrastructure.Persistence.Configurations.Master
{
    /// <summary>
    /// Applied only to <see cref="MasterDbContext"/> — see the namespace-based split in
    /// <c>PhotobizDbContext.OnModelCreating</c> / <c>MasterDbContext.OnModelCreating</c>.
    /// </summary>
    public class TenantConfiguration : IEntityTypeConfiguration<Tenant>
    {
        public void Configure(EntityTypeBuilder<Tenant> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.TenantKey)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.ConnectionString)
                .IsRequired()
                .HasMaxLength(1024);

            builder.Property(x => x.CustomDomain).HasMaxLength(256);
            builder.Property(x => x.DomainVerificationToken).HasMaxLength(64);

            builder.Property(x => x.CustomerEmail)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.CustomerFirstName)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.CustomerLastName)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.Phone)
                .IsRequired()
                .HasMaxLength(32);

            builder.Property(x => x.Address)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.City)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.Country)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.PlanCode)
                .IsRequired()
                .HasMaxLength(64);

            builder.Property(x => x.BillingCycle)
                .IsRequired()
                .HasMaxLength(32);

            builder.Property(x => x.PayHerePaymentId).HasMaxLength(64);
            builder.Property(x => x.SubscriptionId).HasMaxLength(64);
            builder.Property(x => x.PayhereAmount).HasMaxLength(32);
            builder.Property(x => x.PayhereCurrency).HasMaxLength(8);
            builder.Property(x => x.Method).HasMaxLength(32);
            builder.Property(x => x.Recurring).HasMaxLength(8);
            builder.Property(x => x.ItemRecurrence).HasMaxLength(32);
            builder.Property(x => x.ItemDuration).HasMaxLength(32);
            builder.Property(x => x.ItemRecStatus).HasMaxLength(32);
            builder.Property(x => x.ItemRecDateNext).HasMaxLength(32);
            builder.Property(x => x.ItemRecInstallPaid).HasMaxLength(16);
            builder.Property(x => x.CardHolderName).HasMaxLength(128);
            builder.Property(x => x.CardNo).HasMaxLength(32);
            builder.Property(x => x.CardExpiry).HasMaxLength(16);

            // The public key used at login and embedded in the JWT; must be unique across all tenants.
            builder.HasIndex(x => x.TenantKey).IsUnique();

            // Claimed the moment a domain is requested (even before verification), so two tenants
            // can never race to verify the same domain. SQL Server treats multiple NULLs as
            // distinct, so tenants without a custom domain are unaffected.
            builder.HasIndex(x => x.CustomDomain).IsUnique();
        }
    }
}
