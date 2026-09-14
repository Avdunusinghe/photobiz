using Photobiz.Application.Features.SiteThemes.GetSiteTheme;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Entities;
using Photobiz.Domain.Enums;

namespace Photobiz.Application.Tests.Features.SiteThemes.GetSiteTheme
{
    public class GetSiteThemeQueryHandlerTests
    {
        private readonly InMemoryApplicationDbContext _dbContext = InMemoryApplicationDbContext.Create();
        private readonly GetSiteThemeQueryHandler _handler;

        public GetSiteThemeQueryHandlerTests()
        {
            _handler = new GetSiteThemeQueryHandler(_dbContext, TestMappingConfig.Create());
        }

        [Fact]
        public async Task Handle_WhenNoThemeExists_CreatesAndReturnsADefaultTheme()
        {
            var result = await _handler.Handle(new GetSiteThemeQuery(), CancellationToken.None);

            Assert.False(string.IsNullOrWhiteSpace(result.PrimaryColor));
            Assert.False(string.IsNullOrWhiteSpace(result.SecondaryColor));
            Assert.False(string.IsNullOrWhiteSpace(result.AccentColor));
            Assert.Empty(result.FooterLinks);
            Assert.Single(_dbContext.SiteThemes);
        }

        [Fact]
        public async Task Handle_CalledTwice_DoesNotCreateASecondTheme()
        {
            await _handler.Handle(new GetSiteThemeQuery(), CancellationToken.None);
            await _handler.Handle(new GetSiteThemeQuery(), CancellationToken.None);

            Assert.Single(_dbContext.SiteThemes);
        }

        [Fact]
        public async Task Handle_ReturnsFooterLinksOrderedByDisplayOrder()
        {
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
                Url = "https://instagram.com/acme", DisplayOrder = 1,
            });
            theme.FooterLinks.Add(new SiteFooterLink
            {
                Id = Guid.NewGuid(), SiteThemeId = theme.Id, Platform = FooterLinkPlatform.Facebook,
                Url = "https://facebook.com/acme", DisplayOrder = 0,
            });
            _dbContext.SiteThemes.Add(theme);
            await _dbContext.SaveChangesAsync();

            var result = await _handler.Handle(new GetSiteThemeQuery(), CancellationToken.None);

            Assert.Equal(2, result.FooterLinks.Count);
            Assert.Equal(FooterLinkPlatform.Facebook, result.FooterLinks[0].Platform);
            Assert.Equal(FooterLinkPlatform.Instagram, result.FooterLinks[1].Platform);
        }
    }
}
