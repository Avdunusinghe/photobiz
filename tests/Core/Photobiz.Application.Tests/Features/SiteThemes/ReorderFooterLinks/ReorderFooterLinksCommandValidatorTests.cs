using Photobiz.Application.Features.SiteThemes.ReorderFooterLinks;

namespace Photobiz.Application.Tests.Features.SiteThemes.ReorderFooterLinks
{
    public class ReorderFooterLinksCommandValidatorTests
    {
        private readonly ReorderFooterLinksCommandValidator _validator = new();

        [Fact]
        public void Validate_WithDistinctIds_HasNoErrors()
        {
            var result = _validator.Validate(new ReorderFooterLinksCommand([Guid.NewGuid(), Guid.NewGuid()]));

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithAnEmptyList_HasError()
        {
            var result = _validator.Validate(new ReorderFooterLinksCommand([]));

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReorderFooterLinksCommand.OrderedFooterLinkIds));
        }

        [Fact]
        public void Validate_WithDuplicateIds_HasError()
        {
            var id = Guid.NewGuid();

            var result = _validator.Validate(new ReorderFooterLinksCommand([id, id]));

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(ReorderFooterLinksCommand.OrderedFooterLinkIds));
        }
    }
}
