using System.Text.RegularExpressions;
using FluentValidation;

namespace Photobiz.Application.Features.SiteThemes.UpdateSiteTheme
{
    public partial class UpdateSiteThemeCommandValidator : AbstractValidator<UpdateSiteThemeCommand>
    {
        public UpdateSiteThemeCommandValidator()
        {
            RuleFor(x => x.PrimaryColor).Must(BeAHexColor).WithMessage("Enter a valid hex color, e.g. #1A2B3C.");
            RuleFor(x => x.SecondaryColor).Must(BeAHexColor).WithMessage("Enter a valid hex color, e.g. #1A2B3C.");
            RuleFor(x => x.AccentColor).Must(BeAHexColor).WithMessage("Enter a valid hex color, e.g. #1A2B3C.");

            RuleFor(x => x.GradientStartColor).Must(BeNullOrAHexColor).WithMessage("Enter a valid hex color, e.g. #1A2B3C.");
            RuleFor(x => x.GradientEndColor).Must(BeNullOrAHexColor).WithMessage("Enter a valid hex color, e.g. #1A2B3C.");
            RuleFor(x => x)
                .Must(x => (x.GradientStartColor is null) == (x.GradientEndColor is null))
                .WithMessage("Provide both a gradient start and end color, or neither.")
                .WithName(nameof(UpdateSiteThemeCommand.GradientEndColor));

            RuleFor(x => x.GradientDirection).IsInEnum();
            RuleFor(x => x.FontFamily).MaximumLength(64);
            RuleFor(x => x.HeaderStyle).IsInEnum();
            RuleFor(x => x.Tagline).MaximumLength(160);
            RuleFor(x => x.FooterText).MaximumLength(500);
            RuleFor(x => x.FooterCopyrightText).MaximumLength(200);
            RuleFor(x => x.DefaultGalleryTemplate).IsInEnum();
        }

        private static bool BeAHexColor(string value) => HexColorRegex().IsMatch(value ?? string.Empty);

        private static bool BeNullOrAHexColor(string? value) => value is null || HexColorRegex().IsMatch(value);

        [GeneratedRegex("^#[0-9A-Fa-f]{6}$")]
        private static partial Regex HexColorRegex();
    }
}
