using Photobiz.Application.Features.SiteThemes.UpdateFooterLink;
using Photobiz.Domain.Enums;

namespace Photobiz.Application.Tests.Features.SiteThemes.UpdateFooterLink
{
    public class UpdateFooterLinkCommandValidatorTests
    {
        private readonly UpdateFooterLinkCommandValidator _validator = new();

        [Fact]
        public void Validate_WithAValidCommand_HasNoErrors()
        {
            var result = _validator.Validate(
                new UpdateFooterLinkCommand(Guid.NewGuid(), FooterLinkPlatform.Instagram, "https://instagram.com/acme", true));

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithAnInvalidUrl_HasError()
        {
            var result = _validator.Validate(
                new UpdateFooterLinkCommand(Guid.NewGuid(), FooterLinkPlatform.Instagram, "not-a-url", true));

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateFooterLinkCommand.Url));
        }
    }
}
