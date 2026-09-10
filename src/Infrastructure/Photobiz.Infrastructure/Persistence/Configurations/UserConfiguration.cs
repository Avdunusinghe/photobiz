using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Photobiz.Domain.Entities;

namespace Photobiz.Infrastructure.Persistence.Configurations
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Username)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.PasswordHash)
                .IsRequired()
                .HasMaxLength(512);

            builder.Property(x => x.FirstName)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.LastName)
                .IsRequired()
                .HasMaxLength(128);

            builder.Property(x => x.Email)
                .IsRequired()
                .HasMaxLength(256);

            builder.Property(x => x.MobileNumber)
                .HasMaxLength(32);

            builder.Property(x => x.IsActive)
                .IsRequired()
                .HasDefaultValue(true);

            // Soft delete: IsActive == false hides the user from every query. Uniqueness is enforced
            // only among live rows so a deleted user's username / e-mail can be reused.
            builder.HasQueryFilter(x => x.IsActive);

            builder.HasIndex(x => x.Username)
                .IsUnique()
                .HasFilter("[IsActive] = 1");

            builder.HasIndex(x => x.Email)
                .IsUnique()
                .HasFilter("[IsActive] = 1");
        }
    }
}
