using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Photobiz.Domain.Entities;

namespace Photobiz.Infrastructure.Persistence.Configurations
{
    public class SessionTypeConfiguration : IEntityTypeConfiguration<SessionType>
    {
        public void Configure(EntityTypeBuilder<SessionType> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Name)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.Description)
                .HasMaxLength(1024);

            builder.Property(x => x.Price)
                .HasColumnType("decimal(10,2)");
        }
    }
}
