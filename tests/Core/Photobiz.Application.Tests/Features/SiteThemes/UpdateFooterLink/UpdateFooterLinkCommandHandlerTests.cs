using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Features.SiteThemes.AddFooterLink;
using Photobiz.Application.Features.SiteThemes.UpdateFooterLink;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Enums;

namespace Photobiz.Application.Tests.Features.SiteThemes.UpdateFooterLink
{
    public class UpdateFooterLinkCommandHandlerTests
    {
        private readonly InMemoryApplicationDbContext _dbContext = InMemoryApplicationDbContext.Create();
        private readonly UpdateFooterLinkCommandHandler _handler;

        public UpdateFooterLinkCommandHandlerTests()
        {
            _handler = new UpdateFooterLinkCommandHandler(_dbContext, TestMappingConfig.Create());
        }

        private async Task<Guid> SeedLinkAsync()
        {
            var addHandler = new AddFooterLinkCommandHandler(_dbContext, TestMappingConfig.Create());
            var result = await addHandler.Handle(
                new AddFooterLinkCommand(FooterLinkPlatform.Facebook, "https://facebook.com/acme"),
                CancellationToken.None);
            return result.Data!.FooterLinks[0].Id;
        }

        [Fact]
        public async Task Handle_UpdatesTheLinksFieldsAndReturnsTheRefreshedTheme()
        {
            var linkId = await SeedLinkAsync();

            var result = await _handler.Handle(
                new UpdateFooterLinkCommand(linkId, FooterLinkPlatform.Instagram, "https://instagram.com/acme", false),
                CancellationToken.None);

            var link = Assert.Single(result.Data!.FooterLinks);
            Assert.Equal(FooterLinkPlatform.Instagram, link.Platform);
            Assert.Equal("https://instagram.com/acme", link.Url);
            Assert.False(link.IsActive);
        }

        [Fact]
        public async Task Handle_WithAnUnknownId_ThrowsNotFoundException()
        {
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(
                    new UpdateFooterLinkCommand(Guid.NewGuid(), FooterLinkPlatform.Instagram, "https://instagram.com/acme", true),
                    CancellationToken.None));
        }
    }
}
