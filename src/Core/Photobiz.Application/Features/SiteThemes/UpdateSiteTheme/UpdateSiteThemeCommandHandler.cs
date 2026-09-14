using Mapster;
using MediatR;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.SiteThemes.Common;

namespace Photobiz.Application.Features.SiteThemes.UpdateSiteTheme
{
    public class UpdateSiteThemeCommandHandler : IRequestHandler<UpdateSiteThemeCommand, ResultDto<SiteThemeDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly TypeAdapterConfig _mappingConfig;

        public UpdateSiteThemeCommandHandler(IApplicationDbContext dbContext, TypeAdapterConfig mappingConfig)
        {
            _dbContext = dbContext;
            _mappingConfig = mappingConfig;
        }

        public async Task<ResultDto<SiteThemeDto>> Handle(UpdateSiteThemeCommand request, CancellationToken cancellationToken)
        {
            var theme = await SiteThemeAccessor.GetOrCreateAsync(_dbContext, cancellationToken);

            theme.PrimaryColor = request.PrimaryColor;
            theme.SecondaryColor = request.SecondaryColor;
            theme.AccentColor = request.AccentColor;
            theme.GradientStartColor = request.GradientStartColor;
            theme.GradientEndColor = request.GradientEndColor;
            theme.GradientDirection = request.GradientDirection;
            theme.FontFamily = request.FontFamily;
            theme.HeaderStyle = request.HeaderStyle;
            theme.Tagline = request.Tagline;
            theme.FooterText = request.FooterText;
            theme.FooterCopyrightText = request.FooterCopyrightText;
            theme.DefaultGalleryTemplate = request.DefaultGalleryTemplate;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ResultDto<SiteThemeDto>.Succeeded(
                theme.Adapt<SiteThemeDto>(_mappingConfig),
                "Theme updated successfully.");
        }
    }
}
