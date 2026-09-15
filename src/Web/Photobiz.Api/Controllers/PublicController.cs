using MediatR;
using Microsoft.AspNetCore.Mvc;
using Photobiz.Application.Features.Public.GetPublicSite;
using Photobiz.Application.Features.Public.GetTenantDiagnostics;

namespace Photobiz.Api.Controllers
{
    /// <summary>
    /// Anonymous endpoints reachable from a tenant's public site. No <c>[Authorize]</c> here by
    /// design — this is the surface <c>TenantSelectionMiddleware</c> resolves from the request's
    /// <c>Host</c> header rather than a JWT (see the "public path" branch there).
    /// </summary>
    [ApiController]
    [Route("api/public")]
    public class PublicController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PublicController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>
        /// Diagnostic endpoint: echoes the Host header this request arrived with alongside a
        /// harmless slice of whichever tenant database that Host resolved to, so tenant resolution
        /// can be verified end-to-end without a real portfolio-content feature to test against yet.
        /// </summary>
        [HttpGet("whoami")]
        public async Task<ActionResult<WhoAmIResponse>> WhoAmI(CancellationToken cancellationToken)
        {
            var diagnostics = await _mediator.Send(new GetTenantDiagnosticsQuery(), cancellationToken);

            return Ok(new WhoAmIResponse(Request.Host.Value ?? string.Empty, diagnostics.UserCount, diagnostics.SampleUsernames));
        }

        /// <summary>Everything the tenant's public portfolio site (Photobiz.PortfolioApp) needs to render a page.</summary>
        [HttpGet("site")]
        public async Task<ActionResult<PublicSiteDto>> GetSite(CancellationToken cancellationToken)
        {
            var site = await _mediator.Send(new GetPublicSiteQuery(Request.Host.Value ?? string.Empty), cancellationToken);

            return Ok(site);
        }
    }

    public record WhoAmIResponse(string ResolvedHost, int UserCount, IReadOnlyList<string> SampleUsernames);
}
