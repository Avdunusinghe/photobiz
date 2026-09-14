using MediatR;
using Photobiz.Application.Features.SiteThemes.Common;

namespace Photobiz.Application.Features.SiteThemes.GetSiteTheme
{
    /// <summary>Fetches the calling tenant's portfolio site theme (and its footer links).</summary>
    public record GetSiteThemeQuery : IRequest<SiteThemeDto>;
}
