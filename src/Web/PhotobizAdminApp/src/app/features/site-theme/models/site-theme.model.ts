/** Mirrors `Photobiz.Domain.Enums.GradientDirection`. */
export type GradientDirection = 'ToRight' | 'ToBottom' | 'Diagonal';

/** Mirrors `Photobiz.Domain.Enums.HeaderStyle`. */
export type HeaderStyle = 'Classic' | 'Centered' | 'TransparentOverHero';

/** Mirrors `Photobiz.Domain.Enums.GalleryTemplate`. */
export type GalleryTemplate = 'Grid' | 'Masonry' | 'Carousel' | 'Slideshow';

/** Mirrors `Photobiz.Domain.Enums.FooterLinkPlatform`. */
export type FooterLinkPlatform =
  | 'Facebook'
  | 'Instagram'
  | 'X'
  | 'Pinterest'
  | 'LinkedIn'
  | 'YouTube'
  | 'TikTok'
  | 'Website';

/** Mirrors `Photobiz.Application.Common.Models.ResultDto` / `ResultDto<T>`. */
export interface ResultDto<T = never> {
  success: boolean;
  message: string | null;
  data?: T;
}

/** Mirrors `Photobiz.Application.Features.SiteThemes.Common.FooterLinkDto`. */
export interface FooterLinkDto {
  id: string;
  platform: FooterLinkPlatform;
  url: string;
  displayOrder: number;
  isActive: boolean;
}

/** Mirrors `Photobiz.Application.Features.SiteThemes.Common.SiteThemeDto`. */
export interface SiteThemeDto {
  id: string;
  primaryColor: string;
  secondaryColor: string;
  accentColor: string;
  gradientStartColor: string | null;
  gradientEndColor: string | null;
  gradientDirection: GradientDirection;
  fontFamily: string | null;
  headerStyle: HeaderStyle;
  tagline: string | null;
  footerText: string | null;
  footerCopyrightText: string | null;
  defaultGalleryTemplate: GalleryTemplate;
  footerLinks: FooterLinkDto[];
}

/** Body for `PUT /api/tenant/theme`. */
export interface UpdateSiteThemeRequest {
  primaryColor: string;
  secondaryColor: string;
  accentColor: string;
  gradientStartColor: string | null;
  gradientEndColor: string | null;
  gradientDirection: GradientDirection;
  fontFamily: string | null;
  headerStyle: HeaderStyle;
  tagline: string | null;
  footerText: string | null;
  footerCopyrightText: string | null;
  defaultGalleryTemplate: GalleryTemplate;
}

/** Body for `POST /api/tenant/theme/footer-links`. */
export interface AddFooterLinkRequest {
  platform: FooterLinkPlatform;
  url: string;
}

/** Body for `PUT /api/tenant/theme/footer-links/{id}`. */
export interface UpdateFooterLinkRequest {
  platform: FooterLinkPlatform;
  url: string;
  isActive: boolean;
}

/** Body for `PUT /api/tenant/theme/footer-links/reorder`. */
export interface ReorderFooterLinksRequest {
  orderedFooterLinkIds: string[];
}
