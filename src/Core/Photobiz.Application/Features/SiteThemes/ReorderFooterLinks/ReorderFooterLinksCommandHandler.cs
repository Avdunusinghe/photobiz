using FluentValidation;
using Mapster;
using MediatR;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.SiteThemes.Common;

namespace Photobiz.Application.Features.SiteThemes.ReorderFooterLinks
{
    public class ReorderFooterLinksCommandHandler : IRequestHandler<ReorderFooterLinksCommand, ResultDto<SiteThemeDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly TypeAdapterConfig _mappingConfig;

        public ReorderFooterLinksCommandHandler(IApplicationDbContext dbContext, TypeAdapterConfig mappingConfig)
        {
            _dbContext = dbContext;
            _mappingConfig = mappingConfig;
        }

        public async Task<ResultDto<SiteThemeDto>> Handle(ReorderFooterLinksCommand request, CancellationToken cancellationToken)
        {
            var theme = await SiteThemeAccessor.GetOrCreateAsync(_dbContext, cancellationToken);

            var currentIds = theme.FooterLinks.Select(link => link.Id).ToHashSet();
            var requestedIds = request.OrderedFooterLinkIds.ToHashSet();

            if (!currentIds.SetEquals(requestedIds))
            {
                throw new ValidationException(
                    "The footer link order must contain exactly the current set of footer links.");
            }

            var linksById = theme.FooterLinks.ToDictionary(link => link.Id);
            for (var index = 0; index < request.OrderedFooterLinkIds.Count; index++)
            {
                linksById[request.OrderedFooterLinkIds[index]].DisplayOrder = index;
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ResultDto<SiteThemeDto>.Succeeded(
                theme.Adapt<SiteThemeDto>(_mappingConfig),
                "Footer link order updated successfully.");
        }
    }
}
