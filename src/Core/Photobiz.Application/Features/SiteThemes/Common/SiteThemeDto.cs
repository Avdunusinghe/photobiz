using Photobiz.Domain.Enums;

namespace Photobiz.Application.Features.SiteThemes.Common
{
    /// <summary>The tenant's portfolio site appearance, as shown/edited on the "Website Theme" admin screen.</summary>
    public record SiteThemeDto(
        Guid Id,
        string PrimaryColor,
        string SecondaryColor,
        string AccentColor,
        string? GradientStartColor,
        string? GradientEndColor,
        GradientDirection GradientDirection,
        string? FontFamily,
        HeaderStyle HeaderStyle,
        string? Tagline,
        string? FooterText,
        string? FooterCopyrightText,
        GalleryTemplate DefaultGalleryTemplate,
        IReadOnlyList<FooterLinkDto> FooterLinks);
}
