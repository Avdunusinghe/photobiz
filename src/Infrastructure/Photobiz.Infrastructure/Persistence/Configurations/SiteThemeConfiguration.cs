using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Photobiz.Domain.Entities;

namespace Photobiz.Infrastructure.Persistence.Configurations
{
    public class SiteThemeConfiguration : IEntityTypeConfiguration<SiteTheme>
    {
        public void Configure(EntityTypeBuilder<SiteTheme> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.PrimaryColor)
                .IsRequired()
                .HasMaxLength(7);

            builder.Property(x => x.SecondaryColor)
                .IsRequired()
                .HasMaxLength(7);

            builder.Property(x => x.AccentColor)
                .IsRequired()
                .HasMaxLength(7);

            builder.Property(x => x.GradientStartColor)
                .HasMaxLength(7);

            builder.Property(x => x.GradientEndColor)
                .HasMaxLength(7);

            builder.Property(x => x.GradientDirection)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.FontFamily)
                .HasMaxLength(64);

            builder.Property(x => x.HeaderStyle)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.Property(x => x.Tagline)
                .HasMaxLength(160);

            builder.Property(x => x.FooterText)
                .HasMaxLength(500);

            builder.Property(x => x.FooterCopyrightText)
                .HasMaxLength(200);

            builder.Property(x => x.DefaultGalleryTemplate)
                .HasConversion<string>()
                .HasMaxLength(20);

            builder.HasMany(x => x.FooterLinks)
                .WithOne(x => x.SiteTheme)
                .HasForeignKey(x => x.SiteThemeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
