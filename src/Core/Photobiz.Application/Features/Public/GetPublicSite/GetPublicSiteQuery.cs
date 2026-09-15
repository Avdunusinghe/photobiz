using MediatR;

namespace Photobiz.Application.Features.Public.GetPublicSite
{
    /// <summary>
    /// Fetches everything the tenant's public portfolio site needs to render — resolved from the
    /// request's own Host header (the caller reads <c>Request.Host</c> and passes it through),
    /// exactly like <c>TenantSelectionMiddleware</c> already does for this same anonymous path.
    /// </summary>
    public record GetPublicSiteQuery(string Host) : IRequest<PublicSiteDto>;
}
