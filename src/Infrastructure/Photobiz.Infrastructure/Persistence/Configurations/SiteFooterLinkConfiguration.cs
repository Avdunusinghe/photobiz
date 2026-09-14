using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Photobiz.Domain.Entities;

namespace Photobiz.Infrastructure.Persistence.Configurations
{
    public class SiteFooterLinkConfiguration : IEntityTypeConfiguration<SiteFooterLink>
    {
        public void Configure(EntityTypeBuilder<SiteFooterLink> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Platform)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.Url)
                .IsRequired()
                .HasMaxLength(500);
        }
    }
}
