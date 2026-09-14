using MediatR;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.SiteThemes.Common;
using Photobiz.Domain.Enums;

namespace Photobiz.Application.Features.SiteThemes.UpdateFooterLink
{
    public record UpdateFooterLinkCommand(
        Guid Id,
        FooterLinkPlatform Platform,
        string Url,
        bool IsActive) : IRequest<ResultDto<SiteThemeDto>>;
}
