using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.SiteThemes.Common;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Features.SiteThemes.RemoveFooterLink
{
    public class RemoveFooterLinkCommandHandler : IRequestHandler<RemoveFooterLinkCommand, ResultDto<SiteThemeDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly TypeAdapterConfig _mappingConfig;

        public RemoveFooterLinkCommandHandler(IApplicationDbContext dbContext, TypeAdapterConfig mappingConfig)
        {
            _dbContext = dbContext;
            _mappingConfig = mappingConfig;
        }

        public async Task<ResultDto<SiteThemeDto>> Handle(RemoveFooterLinkCommand request, CancellationToken cancellationToken)
        {
            var link = await _dbContext.SiteFooterLinks
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(SiteFooterLink), request.Id);

            _dbContext.SiteFooterLinks.Remove(link);
            await _dbContext.SaveChangesAsync(cancellationToken);

            var theme = await SiteThemeAccessor.GetOrCreateAsync(_dbContext, cancellationToken);

            return ResultDto<SiteThemeDto>.Succeeded(
                theme.Adapt<SiteThemeDto>(_mappingConfig),
                "Footer link removed successfully.");
        }
    }
}
