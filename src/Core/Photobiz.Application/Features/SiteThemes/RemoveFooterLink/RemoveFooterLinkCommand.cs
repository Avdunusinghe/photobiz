using MediatR;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.SiteThemes.Common;

namespace Photobiz.Application.Features.SiteThemes.RemoveFooterLink
{
    public record RemoveFooterLinkCommand(Guid Id) : IRequest<ResultDto<SiteThemeDto>>;
}
