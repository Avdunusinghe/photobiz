using Mapster;
using MediatR;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Features.SiteThemes.Common;

namespace Photobiz.Application.Features.SiteThemes.GetSiteTheme
{
    public class GetSiteThemeQueryHandler : IRequestHandler<GetSiteThemeQuery, SiteThemeDto>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly TypeAdapterConfig _mappingConfig;

        public GetSiteThemeQueryHandler(IApplicationDbContext dbContext, TypeAdapterConfig mappingConfig)
        {
            _dbContext = dbContext;
            _mappingConfig = mappingConfig;
        }

        public async Task<SiteThemeDto> Handle(GetSiteThemeQuery request, CancellationToken cancellationToken)
        {
            var theme = await SiteThemeAccessor.GetOrCreateAsync(_dbContext, cancellationToken);

            return theme.Adapt<SiteThemeDto>(_mappingConfig);
        }
    }
}
