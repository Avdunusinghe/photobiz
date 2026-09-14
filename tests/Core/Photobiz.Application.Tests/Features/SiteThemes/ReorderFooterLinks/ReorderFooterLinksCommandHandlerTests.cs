using FluentValidation;
using Photobiz.Application.Features.SiteThemes.AddFooterLink;
using Photobiz.Application.Features.SiteThemes.ReorderFooterLinks;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Enums;

namespace Photobiz.Application.Tests.Features.SiteThemes.ReorderFooterLinks
{
    public class ReorderFooterLinksCommandHandlerTests
    {
        private readonly InMemoryApplicationDbContext _dbContext = InMemoryApplicationDbContext.Create();
        private readonly ReorderFooterLinksCommandHandler _handler;

        public ReorderFooterLinksCommandHandlerTests()
        {
            _handler = new ReorderFooterLinksCommandHandler(_dbContext, TestMappingConfig.Create());
        }

        private async Task<(Guid First, Guid Second)> SeedTwoLinksAsync()
        {
            var addHandler = new AddFooterLinkCommandHandler(_dbContext, TestMappingConfig.Create());
            await addHandler.Handle(new AddFooterLinkCommand(FooterLinkPlatform.Facebook, "https://facebook.com/acme"), CancellationToken.None);
            var result = await addHandler.Handle(new AddFooterLinkCommand(FooterLinkPlatform.Instagram, "https://instagram.com/acme"), CancellationToken.None);
            return (result.Data!.FooterLinks[0].Id, result.Data.FooterLinks[1].Id);
        }

        [Fact]
        public async Task Handle_ReassignsDisplayOrderToMatchTheGivenSequence()
        {
            var (first, second) = await SeedTwoLinksAsync();

            var result = await _handler.Handle(
                new ReorderFooterLinksCommand([second, first]), CancellationToken.None);

            Assert.Equal(second, result.Data!.FooterLinks[0].Id);
            Assert.Equal(0, result.Data.FooterLinks[0].DisplayOrder);
            Assert.Equal(first, result.Data.FooterLinks[1].Id);
            Assert.Equal(1, result.Data.FooterLinks[1].DisplayOrder);
        }

        [Fact]
        public async Task Handle_WhenTheGivenIdsDoNotMatchTheCurrentLinks_ThrowsValidationException()
        {
            await SeedTwoLinksAsync();

            await Assert.ThrowsAsync<ValidationException>(() =>
                _handler.Handle(new ReorderFooterLinksCommand([Guid.NewGuid()]), CancellationToken.None));
        }
    }
}
