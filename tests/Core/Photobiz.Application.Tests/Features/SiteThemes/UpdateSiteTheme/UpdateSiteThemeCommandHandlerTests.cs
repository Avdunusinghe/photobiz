using Photobiz.Application.Features.SiteThemes.UpdateSiteTheme;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Enums;

namespace Photobiz.Application.Tests.Features.SiteThemes.UpdateSiteTheme
{
    public class UpdateSiteThemeCommandHandlerTests
    {
        private readonly InMemoryApplicationDbContext _dbContext = InMemoryApplicationDbContext.Create();
        private readonly UpdateSiteThemeCommandHandler _handler;

        public UpdateSiteThemeCommandHandlerTests()
        {
            _handler = new UpdateSiteThemeCommandHandler(_dbContext, TestMappingConfig.Create());
        }

        private static UpdateSiteThemeCommand Command() => new(
            PrimaryColor: "#000000",
            SecondaryColor: "#111111",
            AccentColor: "#222222",
            GradientStartColor: "#333333",
            GradientEndColor: "#444444",
            GradientDirection: GradientDirection.Diagonal,
            FontFamily: "Playfair Display",
            HeaderStyle: HeaderStyle.Centered,
            Tagline: "Every moment, captured.",
            FooterText: "Based in Colombo.",
            FooterCopyrightText: "All rights reserved.",
            DefaultGalleryTemplate: GalleryTemplate.Masonry);

        [Fact]
        public async Task Handle_WhenNoThemeExistsYet_CreatesOneWithTheGivenValues()
        {
            var result = await _handler.Handle(Command(), CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal("Theme updated successfully.", result.Message);
            Assert.Equal("#000000", result.Data!.PrimaryColor);
            Assert.Equal(GradientDirection.Diagonal, result.Data.GradientDirection);
            Assert.Equal(HeaderStyle.Centered, result.Data.HeaderStyle);
            Assert.Equal(GalleryTemplate.Masonry, result.Data.DefaultGalleryTemplate);

            Assert.Single(_dbContext.SiteThemes);
        }

        [Fact]
        public async Task Handle_WhenAThemeAlreadyExists_UpdatesTheSameRowRatherThanCreatingAnother()
        {
            await _handler.Handle(Command(), CancellationToken.None);

            await _handler.Handle(Command() with { PrimaryColor = "#ABCDEF" }, CancellationToken.None);

            Assert.Single(_dbContext.SiteThemes);
            Assert.Equal("#ABCDEF", _dbContext.SiteThemes.Single().PrimaryColor);
        }

        [Fact]
        public async Task Handle_CanClearAGradientBySettingBothColorsToNull()
        {
            await _handler.Handle(Command(), CancellationToken.None);

            var result = await _handler.Handle(
                Command() with { GradientStartColor = null, GradientEndColor = null },
                CancellationToken.None);

            Assert.Null(result.Data!.GradientStartColor);
            Assert.Null(result.Data.GradientEndColor);
        }
    }
}
