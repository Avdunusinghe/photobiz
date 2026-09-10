using MediatR;
using Photobiz.Application.Common.Models;

namespace Photobiz.Application.Features.Users.DeleteUser
{
    public record DeleteUserCommand(Guid Id) : IRequest<ResultDto>;
}
