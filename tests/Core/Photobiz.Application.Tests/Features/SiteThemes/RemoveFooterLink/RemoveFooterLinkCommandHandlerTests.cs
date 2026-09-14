using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Features.SiteThemes.AddFooterLink;
using Photobiz.Application.Features.SiteThemes.RemoveFooterLink;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Enums;

namespace Photobiz.Application.Tests.Features.SiteThemes.RemoveFooterLink
{
    public class RemoveFooterLinkCommandHandlerTests
    {
        private readonly InMemoryApplicationDbContext _dbContext = InMemoryApplicationDbContext.Create();
        private readonly RemoveFooterLinkCommandHandler _handler;

        public RemoveFooterLinkCommandHandlerTests()
        {
            _handler = new RemoveFooterLinkCommandHandler(_dbContext, TestMappingConfig.Create());
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
        public async Task Handle_RemovesTheLink()
        {
            var linkId = await SeedLinkAsync();

            var result = await _handler.Handle(new RemoveFooterLinkCommand(linkId), CancellationToken.None);

            Assert.True(result.Success);
            Assert.Empty(result.Data!.FooterLinks);
            Assert.Empty(_dbContext.SiteFooterLinks);
        }

        [Fact]
        public async Task Handle_WithAnUnknownId_ThrowsNotFoundException()
        {
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(new RemoveFooterLinkCommand(Guid.NewGuid()), CancellationToken.None));
        }
    }
}
