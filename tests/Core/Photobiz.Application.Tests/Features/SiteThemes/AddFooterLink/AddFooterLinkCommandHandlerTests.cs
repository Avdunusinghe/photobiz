using Photobiz.Application.Features.SiteThemes.AddFooterLink;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Enums;

namespace Photobiz.Application.Tests.Features.SiteThemes.AddFooterLink
{
    public class AddFooterLinkCommandHandlerTests
    {
        private readonly InMemoryApplicationDbContext _dbContext = InMemoryApplicationDbContext.Create();
        private readonly AddFooterLinkCommandHandler _handler;

        public AddFooterLinkCommandHandlerTests()
        {
            _handler = new AddFooterLinkCommandHandler(_dbContext, TestMappingConfig.Create());
        }

        [Fact]
        public async Task Handle_AddsALinkAndAssignsDisplayOrderZeroWhenItIsTheFirst()
        {
            var result = await _handler.Handle(
                new AddFooterLinkCommand(FooterLinkPlatform.Facebook, "https://facebook.com/acme"),
                CancellationToken.None);

            Assert.True(result.Success);
            var link = Assert.Single(result.Data!.FooterLinks);
            Assert.Equal(FooterLinkPlatform.Facebook, link.Platform);
            Assert.Equal("https://facebook.com/acme", link.Url);
            Assert.Equal(0, link.DisplayOrder);
            Assert.True(link.IsActive);
        }

        [Fact]
        public async Task Handle_AppendsSubsequentLinksAfterTheExistingMaxDisplayOrder()
        {
            await _handler.Handle(new AddFooterLinkCommand(FooterLinkPlatform.Facebook, "https://facebook.com/acme"), CancellationToken.None);

            var result = await _handler.Handle(
                new AddFooterLinkCommand(FooterLinkPlatform.Instagram, "https://instagram.com/acme"),
                CancellationToken.None);

            Assert.Equal(2, result.Data!.FooterLinks.Count);
            Assert.Equal(1, result.Data.FooterLinks[1].DisplayOrder);
        }
    }
}
