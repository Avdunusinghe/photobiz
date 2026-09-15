using NSubstitute;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Features.Public.GetPublicSite;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Entities;
using Photobiz.Domain.Enums;

namespace Photobiz.Application.Tests.Features.Public.GetPublicSite
{
    public class GetPublicSiteQueryHandlerTests
    {
        private readonly InMemoryApplicationDbContext _dbContext = InMemoryApplicationDbContext.Create();
        private readonly ITenantService _tenantService = Substitute.For<ITenantService>();
        private readonly GetPublicSiteQueryHandler _handler;

        public GetPublicSiteQueryHandlerTests()
        {
            _handler = new GetPublicSiteQueryHandler(_dbContext, _tenantService);
        }

        private void StubTenant(string host = "acme.photobiz.test", string name = "Acme Studio", string? logoUrl = null) =>
            _tenantService.GetPublicTenantByHostAsync(host, Arg.Any<CancellationToken>())
                .Returns(new PublicTenantInfo(name, logoUrl));

        [Fact]
        public async Task Handle_WithAnUnknownHost_ThrowsTenantNotFoundException()
        {
            _tenantService.GetPublicTenantByHostAsync("ghost.test", Arg.Any<CancellationToken>())
                .Returns((PublicTenantInfo?)null);

            await Assert.ThrowsAsync<TenantNotFoundException>(() =>
                _handler.Handle(new GetPublicSiteQuery("ghost.test"), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WhenNoThemeExistsYet_CreatesAndReturnsADefaultTheme()
        {
            StubTenant(logoUrl: "https://cdn.acme.test/logo.webp");

            var result = await _handler.Handle(new GetPublicSiteQuery("acme.photobiz.test"), CancellationToken.None);

            Assert.Equal("Acme Studio", result.TenantName);
            Assert.Equal("https://cdn.acme.test/logo.webp", result.LogoUrl);
            Assert.False(string.IsNullOrWhiteSpace(result.PrimaryColor));
            Assert.Empty(result.Galleries);
        }

        [Fact]
        public async Task Handle_OnlyIncludesActiveFooterLinksOrderedByDisplayOrder()
        {
            StubTenant();
            var theme = new SiteTheme
            {
                Id = Guid.NewGuid(),
                PrimaryColor = "#000000",
                SecondaryColor = "#111111",
                AccentColor = "#222222",
            };
            theme.FooterLinks.Add(new SiteFooterLink
            {
                Id = Guid.NewGuid(), SiteThemeId = theme.Id, Platform = FooterLinkPlatform.Instagram,
                Url = "https://instagram.com/acme", DisplayOrder = 1, IsActive = true,
            });
            theme.FooterLinks.Add(new SiteFooterLink
            {
                Id = Guid.NewGuid(), SiteThemeId = theme.Id, Platform = FooterLinkPlatform.Facebook,
                Url = "https://facebook.com/acme", DisplayOrder = 0, IsActive = true,
            });
            theme.FooterLinks.Add(new SiteFooterLink
            {
                Id = Guid.NewGuid(), SiteThemeId = theme.Id, Platform = FooterLinkPlatform.TikTok,
                Url = "https://tiktok.com/acme", DisplayOrder = 2, IsActive = false,
            });
            _dbContext.SiteThemes.Add(theme);
            await _dbContext.SaveChangesAsync();

            var result = await _handler.Handle(new GetPublicSiteQuery("acme.photobiz.test"), CancellationToken.None);

            Assert.Equal(2, result.FooterLinks.Count);
            Assert.Equal(FooterLinkPlatform.Facebook, result.FooterLinks[0].Platform);
            Assert.Equal(FooterLinkPlatform.Instagram, result.FooterLinks[1].Platform);
        }

        [Fact]
        public async Task Handle_ReturnsGalleriesWithTheirPhotosOrderedByTitle()
        {
            StubTenant();
            var user = new User
            {
                Id = Guid.NewGuid(), Username = "admin", PasswordHash = "x",
                FirstName = "A", LastName = "B", Email = "a@b.test", IsActive = true,
            };
            var galleryB = new Gallery { Id = Guid.NewGuid(), Title = "B Wedding", UserId = user.Id };
            galleryB.Photos.Add(new Photo
            {
                Id = Guid.NewGuid(), GalleryId = galleryB.Id,
                ThumbnailUrl = "thumb.jpg", MediumUrl = "medium.jpg", FullUrl = "full.jpg",
            });
            var galleryA = new Gallery { Id = Guid.NewGuid(), Title = "A Portraits", UserId = user.Id };
            _dbContext.Users.Add(user);
            _dbContext.Galleries.AddRange(galleryB, galleryA);
            await _dbContext.SaveChangesAsync();

            var result = await _handler.Handle(new GetPublicSiteQuery("acme.photobiz.test"), CancellationToken.None);

            Assert.Equal(2, result.Galleries.Count);
            Assert.Equal("A Portraits", result.Galleries[0].Title);
            Assert.Equal("B Wedding", result.Galleries[1].Title);
            Assert.Single(result.Galleries[1].Photos);
        }
    }
}
