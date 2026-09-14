using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.SiteThemes.Common;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Features.SiteThemes.UpdateFooterLink
{
    public class UpdateFooterLinkCommandHandler : IRequestHandler<UpdateFooterLinkCommand, ResultDto<SiteThemeDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly TypeAdapterConfig _mappingConfig;

        public UpdateFooterLinkCommandHandler(IApplicationDbContext dbContext, TypeAdapterConfig mappingConfig)
        {
            _dbContext = dbContext;
            _mappingConfig = mappingConfig;
        }

        public async Task<ResultDto<SiteThemeDto>> Handle(UpdateFooterLinkCommand request, CancellationToken cancellationToken)
        {
            var link = await _dbContext.SiteFooterLinks
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken)
                ?? throw new NotFoundException(nameof(SiteFooterLink), request.Id);

            link.Platform = request.Platform;
            link.Url = request.Url;
            link.IsActive = request.IsActive;

            await _dbContext.SaveChangesAsync(cancellationToken);

            var theme = await SiteThemeAccessor.GetOrCreateAsync(_dbContext, cancellationToken);

            return ResultDto<SiteThemeDto>.Succeeded(
                theme.Adapt<SiteThemeDto>(_mappingConfig),
                "Footer link updated successfully.");
        }
    }
}
