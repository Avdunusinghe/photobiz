using Photobiz.Application.Features.SiteThemes.UpdateSiteTheme;
using Photobiz.Domain.Enums;

namespace Photobiz.Application.Tests.Features.SiteThemes.UpdateSiteTheme
{
    public class UpdateSiteThemeCommandValidatorTests
    {
        private readonly UpdateSiteThemeCommandValidator _validator = new();

        private static UpdateSiteThemeCommand Valid() => new(
            PrimaryColor: "#111827",
            SecondaryColor: "#F97316",
            AccentColor: "#2563EB",
            GradientStartColor: null,
            GradientEndColor: null,
            GradientDirection: GradientDirection.ToRight,
            FontFamily: "Inter",
            HeaderStyle: HeaderStyle.Classic,
            Tagline: "Capturing your story",
            FooterText: "Thanks for visiting.",
            FooterCopyrightText: null,
            DefaultGalleryTemplate: GalleryTemplate.Grid);

        [Fact]
        public void Validate_WithValidCommand_HasNoErrors()
        {
            var result = _validator.Validate(Valid());

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("111827")]
        [InlineData("#zzzzzz")]
        [InlineData("#12345")]
        public void Validate_WithInvalidPrimaryColor_HasError(string color)
        {
            var result = _validator.Validate(Valid() with { PrimaryColor = color });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSiteThemeCommand.PrimaryColor));
        }

        [Fact]
        public void Validate_WithGradientStartButNoEnd_HasError()
        {
            var result = _validator.Validate(Valid() with { GradientStartColor = "#ABCDEF" });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSiteThemeCommand.GradientEndColor));
        }

        [Fact]
        public void Validate_WithBothGradientColors_HasNoError()
        {
            var result = _validator.Validate(Valid() with { GradientStartColor = "#ABCDEF", GradientEndColor = "#123456" });

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithUndefinedGradientDirection_HasError()
        {
            var result = _validator.Validate(Valid() with { GradientDirection = (GradientDirection)999 });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSiteThemeCommand.GradientDirection));
        }

        [Fact]
        public void Validate_WithUndefinedHeaderStyle_HasError()
        {
            var result = _validator.Validate(Valid() with { HeaderStyle = (HeaderStyle)999 });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSiteThemeCommand.HeaderStyle));
        }

        [Fact]
        public void Validate_WithTaglineOverTheLimit_HasError()
        {
            var result = _validator.Validate(Valid() with { Tagline = new string('a', 161) });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateSiteThemeCommand.Tagline));
        }
    }
}
