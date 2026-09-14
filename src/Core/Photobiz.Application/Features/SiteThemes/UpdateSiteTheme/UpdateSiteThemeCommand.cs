using MediatR;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.SiteThemes.Common;
using Photobiz.Domain.Enums;

namespace Photobiz.Application.Features.SiteThemes.UpdateSiteTheme
{
    /// <summary>Replaces the calling tenant's portfolio site theme. Footer links are managed separately.</summary>
    public record UpdateSiteThemeCommand(
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
        GalleryTemplate DefaultGalleryTemplate) : IRequest<ResultDto<SiteThemeDto>>;
}
