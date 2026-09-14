using Mapster;
using MediatR;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.SiteThemes.Common;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Features.SiteThemes.AddFooterLink
{
    public class AddFooterLinkCommandHandler : IRequestHandler<AddFooterLinkCommand, ResultDto<SiteThemeDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly TypeAdapterConfig _mappingConfig;

        public AddFooterLinkCommandHandler(IApplicationDbContext dbContext, TypeAdapterConfig mappingConfig)
        {
            _dbContext = dbContext;
            _mappingConfig = mappingConfig;
        }

        public async Task<ResultDto<SiteThemeDto>> Handle(AddFooterLinkCommand request, CancellationToken cancellationToken)
        {
            var theme = await SiteThemeAccessor.GetOrCreateAsync(_dbContext, cancellationToken);

            var nextDisplayOrder = theme.FooterLinks.Count == 0
                ? 0
                : theme.FooterLinks.Max(link => link.DisplayOrder) + 1;

            // Added via the DbSet, not theme.FooterLinks.Add(...) — a manually-assigned Guid key
            // reached only through collection-navigation fixup is tracked as Modified, not Added,
            // and SaveChanges then fails trying to "update" a row that was never inserted.
            _dbContext.SiteFooterLinks.Add(new SiteFooterLink
            {
                Id = Guid.NewGuid(),
                SiteThemeId = theme.Id,
                Platform = request.Platform,
                Url = request.Url,
                DisplayOrder = nextDisplayOrder,
                IsActive = true,
            });

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ResultDto<SiteThemeDto>.Succeeded(
                theme.Adapt<SiteThemeDto>(_mappingConfig),
                "Footer link added successfully.");
        }
    }
}
