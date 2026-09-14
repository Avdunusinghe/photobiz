using MediatR;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.SiteThemes.Common;

namespace Photobiz.Application.Features.SiteThemes.ReorderFooterLinks
{
    /// <summary>Reassigns display order from the given sequence — index 0 becomes first. Must contain exactly the current set of footer link ids.</summary>
    public record ReorderFooterLinksCommand(IReadOnlyList<Guid> OrderedFooterLinkIds) : IRequest<ResultDto<SiteThemeDto>>;
}
