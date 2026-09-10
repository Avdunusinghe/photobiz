using MediatR;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.Users.Common;

namespace Photobiz.Application.Features.Users.UpdateUser
{
    public record UpdateUserCommand(
        Guid Id,
        string Username,
        string FirstName,
        string LastName,
        string Email,
        string? MobileNumber,
        bool IsActive,
        IReadOnlyList<string> Roles,
        string? Password = null) : IRequest<ResultDto<UserDto>>;
}
