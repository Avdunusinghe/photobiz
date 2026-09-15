using MediatR;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Features.SiteThemes.Common;

namespace Photobiz.Application.Features.Public.GetPublicSite
{
    public class GetPublicSiteQueryHandler : IRequestHandler<GetPublicSiteQuery, PublicSiteDto>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly ITenantService _tenantService;

        public GetPublicSiteQueryHandler(IApplicationDbContext dbContext, ITenantService tenantService)
        {
            _dbContext = dbContext;
            _tenantService = tenantService;
        }

        public async Task<PublicSiteDto> Handle(GetPublicSiteQuery request, CancellationToken cancellationToken)
        {
            var tenantInfo = await _tenantService.GetPublicTenantByHostAsync(request.Host, cancellationToken)
                ?? throw new TenantNotFoundException($"No tenant is registered for host '{request.Host}'.");

            var theme = await SiteThemeAccessor.GetOrCreateAsync(_dbContext, cancellationToken);

            var galleries = await _dbContext.Galleries
                .Include(gallery => gallery.Photos)
                .OrderBy(gallery => gallery.Title)
                .ToListAsync(cancellationToken);

            return new PublicSiteDto(
                tenantInfo.Name,
                tenantInfo.LogoUrl,
                theme.PrimaryColor,
                theme.SecondaryColor,
                theme.AccentColor,
                theme.GradientStartColor,
                theme.GradientEndColor,
                theme.GradientDirection,
                theme.FontFamily,
                theme.HeaderStyle,
                theme.Tagline,
                theme.FooterText,
                theme.FooterCopyrightText,
                theme.DefaultGalleryTemplate,
                theme.FooterLinks
                    .Where(link => link.IsActive)
                    .OrderBy(link => link.DisplayOrder)
                    .Select(link => new PublicFooterLinkDto(link.Platform, link.Url))
                    .ToList(),
                galleries
                    .Select(gallery => new PublicGalleryDto(
                        gallery.Id,
                        gallery.Title,
                        gallery.Description,
                        gallery.Template,
                        gallery.Photos
                            .Select(photo => new PublicPhotoDto(
                                photo.Id, photo.ThumbnailUrl, photo.MediumUrl, photo.FullUrl, photo.AltText))
                            .ToList()))
                    .ToList());
        }
    }
}
