using MediatR;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.Users.Common;

namespace Photobiz.Application.Features.Users.CreateUser
{
    public record CreateUserCommand(
        string Username,
        string Password,
        string FirstName,
        string LastName,
        string Email,
        string? MobileNumber,
        bool IsActive,
        IReadOnlyList<string> Roles) : IRequest<ResultDto<UserDto>>;
}
