using MediatR;
using Microsoft.AspNetCore.Mvc;
using Photobiz.Application.Features.Auth.IssueToken;

namespace Photobiz.Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("token")]
        public async Task<ActionResult<IssueTokenResult>> IssueToken(
            [FromBody] TokenRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new IssueTokenCommand(request.Username, request.Password), cancellationToken);
            return Ok(result);
        }
    }

    public record TokenRequest(string Username, string Password);
}
