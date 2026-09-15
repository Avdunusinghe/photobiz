using Photobiz.Domain.Enums;

namespace Photobiz.Application.Features.Public.GetPublicSite
{
    public record PublicPhotoDto(
        Guid Id,
        string ThumbnailUrl,
        string MediumUrl,
        string FullUrl,
        string? AltText);

    public record PublicGalleryDto(
        Guid Id,
        string Title,
        string? Description,
        GalleryTemplate? Template,
        IReadOnlyList<PublicPhotoDto> Photos);

    public record PublicFooterLinkDto(FooterLinkPlatform Platform, string Url);

    /// <summary>
    /// Everything a tenant's public portfolio site needs to render a page: business identity,
    /// theme, footer, and gallery content. Combines the tenant's Master-database profile with its
    /// own tenant-database theme and galleries into one payload for the portfolio app.
    /// </summary>
    public record PublicSiteDto(
        string TenantName,
        string? LogoUrl,
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
        IReadOnlyList<PublicFooterLinkDto> FooterLinks,
        IReadOnlyList<PublicGalleryDto> Galleries);
}
