using MediatR;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.SiteThemes.Common;
using Photobiz.Domain.Enums;

namespace Photobiz.Application.Features.SiteThemes.AddFooterLink
{
    /// <summary>Adds a social/contact link to the calling tenant's portfolio site footer. Display order is assigned automatically — last.</summary>
    public record AddFooterLinkCommand(FooterLinkPlatform Platform, string Url) : IRequest<ResultDto<SiteThemeDto>>;
}
