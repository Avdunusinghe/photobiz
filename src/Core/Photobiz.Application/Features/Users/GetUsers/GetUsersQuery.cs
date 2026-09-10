using MediatR;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.Users.Common;

namespace Photobiz.Application.Features.Users.GetUsers
{
    public record GetUsersQuery(
        string? SearchText = null,
        string? Role = null,
        bool? IsActive = null,
        int PageNumber = 1,
        int PageSize = 20) : IRequest<PagedResult<UserDto>>;
}
