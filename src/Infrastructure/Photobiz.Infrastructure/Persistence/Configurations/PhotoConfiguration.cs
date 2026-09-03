using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Photobiz.Domain.Entities;

namespace Photobiz.Infrastructure.Persistence.Configurations
{
    public class PhotoConfiguration : IEntityTypeConfiguration<Photo>
    {
        public void Configure(EntityTypeBuilder<Photo> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ThumbnailUrl)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(x => x.MediumUrl)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(x => x.FullUrl)
                .IsRequired()
                .HasMaxLength(2048);

            builder.Property(x => x.AltText)
                .HasMaxLength(512);

            builder.HasOne(x => x.Gallery)
                .WithMany(x => x.Photos)
                .HasForeignKey(x => x.GalleryId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
