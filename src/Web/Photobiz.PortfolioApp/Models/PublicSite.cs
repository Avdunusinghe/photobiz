namespace Photobiz.PortfolioApp.Models
{
    // Mirrors Photobiz.Domain.Enums (Photobiz.Api). Deliberately duplicated rather than referenced
    // — this app talks to the API only over HTTP, never via a shared project reference (see the
    // same note on Photobiz.PortfolioSample's PortfolioApiClient).
    public enum GradientDirection { ToRight, ToBottom, Diagonal }

    public enum HeaderStyle { Classic, Centered, TransparentOverHero }

    public enum GalleryTemplate { Grid, Masonry, Carousel, Slideshow }

    public enum FooterLinkPlatform { Facebook, Instagram, X, Pinterest, LinkedIn, YouTube, TikTok, Website }

    public record PublicPhoto(Guid Id, string ThumbnailUrl, string MediumUrl, string FullUrl, string? AltText);

    public record PublicGallery(
        Guid Id,
        string Title,
        string? Description,
        GalleryTemplate? Template,
        IReadOnlyList<PublicPhoto> Photos);

    public record PublicFooterLink(FooterLinkPlatform Platform, string Url);

    /// <summary>Mirrors `Photobiz.Application.Features.Public.GetPublicSite.PublicSiteDto` — the payload behind `GET /api/public/site`.</summary>
    public record PublicSite(
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
        IReadOnlyList<PublicFooterLink> FooterLinks,
        IReadOnlyList<PublicGallery> Galleries);
}
