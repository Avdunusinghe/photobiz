using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.SmtpSettings.Common;
using Photobiz.Application.Features.SmtpSettings.GetSmtpSettings;
using Photobiz.Application.Features.Tenants.Common;
using Photobiz.Application.Features.Tenants.GetTenantDetails;
using Photobiz.Application.Features.Tenants.RemoveTenantLogo;
using Photobiz.Application.Features.Tenants.UpdateTenantDetails;
using Photobiz.Application.Features.Tenants.UploadTenantLogo;
using Photobiz.Domain.Entities;

namespace Photobiz.Api.Controllers
{
    /// <summary>
    /// The calling tenant's own business profile — never another tenant's. There is no id route
    /// parameter; every command/query here always resolves the tenant from the caller's own JWT.
    /// </summary>
    [ApiController]
    [Route("api/tenant")]
    [Authorize(Roles = RoleNames.Admin)]
    public class TenantController : ControllerBase
    {
        private readonly IMediator _mediator;

        public TenantController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<TenantDetailsDto>> GetTenantDetails(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetTenantDetailsQuery(), cancellationToken);

            return Ok(result);
        }

        [HttpPut]
        public async Task<ActionResult<ResultDto<TenantDetailsDto>>> UpdateTenantDetails(
            [FromBody] UpdateTenantDetailsRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateTenantDetailsCommand(
                    request.Name,
                    request.CustomerEmail,
                    request.CustomerFirstName,
                    request.CustomerLastName,
                    request.Phone,
                    request.Address,
                    request.City,
                    request.Country),
                cancellationToken);

            return Ok(result);
        }

        [HttpPost("logo")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<ResultDto<TenantDetailsDto>>> UploadLogo(
            IFormFile file,
            CancellationToken cancellationToken)
        {
            await using var stream = file.OpenReadStream();

            var result = await _mediator.Send(
                new UploadTenantLogoCommand(stream, file.FileName, file.ContentType, file.Length),
                cancellationToken);

            return Ok(result);
        }

        [HttpDelete("logo")]
        public async Task<ActionResult<ResultDto<TenantDetailsDto>>> RemoveLogo(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RemoveTenantLogoCommand(), cancellationToken);

            return Ok(result);
        }

        /// <summary>Returns <c>null</c> (with a 200) when the tenant hasn't configured SMTP yet — that's a normal state, not a 404.</summary>
        [HttpGet("smtp")]
        public async Task<ActionResult<SmtpSettingDto?>> GetSmtpSettings(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetSmtpSettingsQuery(), cancellationToken);

            return Ok(result);
        }
    }

    public record UpdateTenantDetailsRequest(
        string Name,
        string CustomerEmail,
        string CustomerFirstName,
        string CustomerLastName,
        string Phone,
        string Address,
        string City,
        string Country);
}
