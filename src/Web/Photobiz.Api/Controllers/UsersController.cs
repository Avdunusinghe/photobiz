using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.Users.Common;
using Photobiz.Application.Features.Users.CreateUser;
using Photobiz.Application.Features.Users.DeleteUser;
using Photobiz.Application.Features.Users.GetUsers;
using Photobiz.Application.Features.Users.UpdateUser;
using Photobiz.Domain.Entities;

namespace Photobiz.Api.Controllers
{
    [ApiController]
    [Route("api/users")]
    [Authorize(Roles = RoleNames.Admin)]
    public class UsersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public UsersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
            [FromQuery] string? searchText,
            [FromQuery] string? role,
            [FromQuery] bool? isActive,
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var result = await _mediator.Send(
                new GetUsersQuery(searchText, role, isActive, pageNumber, pageSize),
                cancellationToken);

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<ResultDto<UserDto>>> CreateUser(
            [FromBody] CreateUserRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new CreateUserCommand(
                    request.Username,
                    request.Password,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.MobileNumber,
                    request.IsActive,
                    request.Roles ?? []),
                cancellationToken);

            return CreatedAtAction(nameof(GetUsers), new { }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<ActionResult<ResultDto<UserDto>>> UpdateUser(
            Guid id,
            [FromBody] UpdateUserRequest request,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(
                new UpdateUserCommand(
                    id,
                    request.Username,
                    request.FirstName,
                    request.LastName,
                    request.Email,
                    request.MobileNumber,
                    request.IsActive,
                    request.Roles ?? [],
                    request.Password),
                cancellationToken);

            return Ok(result);
        }

        [HttpDelete("{id:guid}")]
        public async Task<ActionResult<ResultDto>> DeleteUser(
            Guid id,
            CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteUserCommand(id), cancellationToken);

            return Ok(result);
        }
    }

    public record CreateUserRequest(
        string Username,
        string Password,
        string FirstName,
        string LastName,
        string Email,
        string? MobileNumber,
        bool IsActive,
        IReadOnlyList<string>? Roles);

    public record UpdateUserRequest(
        string Username,
        string FirstName,
        string LastName,
        string Email,
        string? MobileNumber,
        bool IsActive,
        IReadOnlyList<string>? Roles,
        string? Password);
}
