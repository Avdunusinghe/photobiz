using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Photobiz.Domain.Entities;
using Photobiz.Domain.Enums;
using Photobiz.Infrastructure.Persistence;

namespace Photobiz.Infrastructure.Persistence.Seeding
{
    /// <summary>
    /// Gives the tenant a handful of demo galleries — one per <see cref="GalleryTemplate"/> — so the
    /// portfolio site (Photobiz.PortfolioApp) has real content to render locally. Photo URLs point
    /// at picsum.photos (a public placeholder-image service) purely as seed/demo data; a real tenant
    /// would populate these via a future photo-upload feature.
    /// </summary>
    public static class GallerySeeder
    {
        private static readonly (string Title, string Description, GalleryTemplate? Template, string Seed, int PhotoCount)[] Galleries =
        [
            ("Wedding Highlights", "A selection of favorite moments from recent weddings.", null, "wedding", 6),
            ("Portrait Sessions", "Studio and outdoor portrait work.", GalleryTemplate.Masonry, "portrait", 7),
            ("Engagement Shoots", "Couples, out and about.", GalleryTemplate.Carousel, "engagement", 5),
            ("Studio Sessions", "Behind the scenes in the studio.", GalleryTemplate.Slideshow, "studio", 4),
        ];

        public static async Task SeedAsync(
            PhotobizDbContext dbContext,
            ILogger logger,
            CancellationToken cancellationToken = default)
        {
            if (await dbContext.Galleries.AnyAsync(cancellationToken))
            {
                return;
            }

            var user = await dbContext.Users.OrderBy(u => u.CreatedAt).FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return;
            }

            foreach (var (title, description, template, seed, photoCount) in Galleries)
            {
                var gallery = new Gallery
                {
                    Id = Guid.NewGuid(),
                    Title = title,
                    Description = description,
                    Template = template,
                    UserId = user.Id,
                };

                for (var i = 1; i <= photoCount; i++)
                {
                    var photoSeed = $"{seed}-{i}";
                    gallery.Photos.Add(new Photo
                    {
                        Id = Guid.NewGuid(),
                        GalleryId = gallery.Id,
                        ThumbnailUrl = $"https://picsum.photos/seed/{photoSeed}/400/400",
                        MediumUrl = $"https://picsum.photos/seed/{photoSeed}/900/900",
                        FullUrl = $"https://picsum.photos/seed/{photoSeed}/1600/1600",
                        AltText = $"{title} photo {i}",
                    });
                }

                dbContext.Galleries.Add(gallery);
            }

            await dbContext.SaveChangesAsync(cancellationToken);

            logger.LogInformation("Seeded {Count} demo galleries.", Galleries.Length);
        }
    }
}
