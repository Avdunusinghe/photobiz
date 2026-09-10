using MediatR;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Models;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Features.Users.DeleteUser
{
    public class DeleteUserCommandHandler : IRequestHandler<DeleteUserCommand, ResultDto>
    {
        private readonly IApplicationDbContext _dbContext;

        public DeleteUserCommandHandler(IApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<ResultDto> Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (user is null)
            {
                throw new NotFoundException(nameof(User), request.Id);
            }

            // Soft delete: clearing IsActive hides the row from every query and revokes sign-in.
            // The row and its role assignments are kept so the user can be restored; the audit
            // interceptor stamps UpdatedBy / UpdatedAt with who deleted it and when.
            user.IsActive = false;

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ResultDto.Succeeded("User deleted successfully.");
        }
    }
}
