using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Photobiz.Domain.Entities;

namespace Photobiz.Infrastructure.Persistence.Configurations
{
    public class BookingConfiguration : IEntityTypeConfiguration<Booking>
    {
        public void Configure(EntityTypeBuilder<Booking> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(32);

            builder.HasOne(x => x.Client)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.ClientId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.SessionType)
                .WithMany(x => x.Bookings)
                .HasForeignKey(x => x.SessionTypeId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
