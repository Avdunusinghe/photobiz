using Photobiz.Application.Features.SiteThemes.AddFooterLink;
using Photobiz.Domain.Enums;

namespace Photobiz.Application.Tests.Features.SiteThemes.AddFooterLink
{
    public class AddFooterLinkCommandValidatorTests
    {
        private readonly AddFooterLinkCommandValidator _validator = new();

        [Fact]
        public void Validate_WithAValidCommand_HasNoErrors()
        {
            var result = _validator.Validate(new AddFooterLinkCommand(FooterLinkPlatform.Instagram, "https://instagram.com/acme"));

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-a-url")]
        [InlineData("ftp://acme.test")]
        public void Validate_WithAnInvalidUrl_HasError(string url)
        {
            var result = _validator.Validate(new AddFooterLinkCommand(FooterLinkPlatform.Instagram, url));

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddFooterLinkCommand.Url));
        }

        [Fact]
        public void Validate_WithAnUndefinedPlatform_HasError()
        {
            var result = _validator.Validate(new AddFooterLinkCommand((FooterLinkPlatform)999, "https://acme.test"));

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(AddFooterLinkCommand.Platform));
        }
    }
}
