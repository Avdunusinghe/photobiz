using Photobiz.Domain.Enums;

namespace Photobiz.Application.Features.SiteThemes.Common
{
    public record FooterLinkDto(
        Guid Id,
        FooterLinkPlatform Platform,
        string Url,
        int DisplayOrder,
        bool IsActive);
}
