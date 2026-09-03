using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Photobiz.Domain.Entities;

namespace Photobiz.Infrastructure.Persistence.Configurations
{
    public class GalleryConfiguration : IEntityTypeConfiguration<Gallery>
    {
        public void Configure(EntityTypeBuilder<Gallery> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Title)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.Description)
                .HasMaxLength(2000);

            builder.HasOne(x => x.User)
                .WithMany(x => x.Galleries)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
