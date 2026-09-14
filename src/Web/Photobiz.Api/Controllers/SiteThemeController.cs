using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.SiteThemes.AddFooterLink;
using Photobiz.Application.Features.SiteThemes.Common;
using Photobiz.Application.Features.SiteThemes.GetSiteTheme;
using Photobiz.Application.Features.SiteThemes.RemoveFooterLink;
using Photobiz.Application.Features.SiteThemes.ReorderFooterLinks;
using Photobiz.Application.Features.SiteThemes.UpdateFooterLink;
using Photobiz.Application.Features.SiteThemes.UpdateSiteTheme;
using Photobiz.Domain.Entities;
using Photobiz.Domain.Enums;

namespace Photobiz.Api.Controllers
{
    /// <summary>
    /// The calling tenant's own portfolio site theme and footer links — never another tenant's.
    /// There is no id route parameter; everything here always resolves to the caller's own tenant
    /// database (via the JWT-selected connection, already applied by <c>TenantSelectionMiddleware</c>).
    /// </summary>
    [ApiController]
    [Route("api/tenant/theme")]
    [Authorize(Roles = RoleNames.Admin)]
    public class SiteThemeController : ControllerBase
    {
        private readonly IMediator _mediator;

        public SiteThemeController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<SiteThemeDto>> GetTheme(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSiteThemeQuery(), cancellationToken);

            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<ResultDto<SiteThemeDto>>> UpdateTheme(
            [FromBody] UpdateSiteThemeRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateSiteThemeCommand(
                    request.PrimaryColor,
                    request.SecondaryColor,
                    request.AccentColor,
                    request.GradientStartColor,
                    request.GradientEndColor,
                    request.GradientDirection,
                    request.FontFamily,
                    request.HeaderStyle,
                    request.Tagline,
                    request.FooterText,
                    request.FooterCopyrightText,
                    request.DefaultGalleryTemplate),
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("footer-links")]
        public async Task<ActionResult<ResultDto<SiteThemeDto>>> AddFooterLink(
            [FromBody] AddFooterLinkRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new AddFooterLinkCommand(request.Platform, request.Url), cancellationToken);

            return Ok(result);
        }

        [HttpPut("footer-links/{id:guid}")]
        public async Task<ActionResult<ResultDto<SiteThemeDto>>> UpdateFooterLink(
            Guid id,
            [FromBody] UpdateFooterLinkRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateFooterLinkCommand(id, request.Platform, request.Url, request.IsActive),
                cancellationToken);

            return Ok(result);
        }

        [HttpDelete("footer-links/{id:guid}")]
        public async Task<ActionResult<ResultDto<SiteThemeDto>>> RemoveFooterLink(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RemoveFooterLinkCommand(id), cancellationToken);

            return Ok(result);
        }

        [HttpPut("footer-links/reorder")]
        public async Task<ActionResult<ResultDto<SiteThemeDto>>> ReorderFooterLinks(
            [FromBody] ReorderFooterLinksRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new ReorderFooterLinksCommand(request.OrderedFooterLinkIds), cancellationToken);

            return Ok(result);
        }
    }

    public record UpdateSiteThemeRequest(
        string PrimaryColor,
        string SecondaryColor,
        string AccentColor,
        string? GradientStartColor,
        string? GradientEndColor,
        GradientDirection GradientDirection,
        string? FontFamily,
        HeaderStyle HeaderStyle,
        string? Tagline,
        string? FooterText,
        string? FooterCopyrightText,
        GalleryTemplate DefaultGalleryTemplate);

    public record AddFooterLinkRequest(FooterLinkPlatform Platform, string Url);

    public record UpdateFooterLinkRequest(FooterLinkPlatform Platform, string Url, bool IsActive);

    public record ReorderFooterLinksRequest(IReadOnlyList<Guid> OrderedFooterLinkIds);
}
