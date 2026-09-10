using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.Users.Common;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Features.Users.UpdateUser
{
    public class UpdateUserCommandHandler : IRequestHandler<UpdateUserCommand, ResultDto<UserDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly TypeAdapterConfig _mappingConfig;

        public UpdateUserCommandHandler(
            IApplicationDbContext dbContext,
            IPasswordHasher<User> passwordHasher,
            TypeAdapterConfig mappingConfig)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _mappingConfig = mappingConfig;
        }

        public async Task<ResultDto<UserDto>> Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _dbContext.Users
                .Include(x => x.UserRoles)
                .ThenInclude(x => x.Role)
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (user is null)
            {
                throw new NotFoundException(nameof(User), request.Id);
            }

            var username = request.Username.Trim();
            var email = request.Email.Trim();

            var usernameTaken = await _dbContext.Users
                .AnyAsync(x => x.Id != request.Id && x.Username == username, cancellationToken);

            if (usernameTaken)
            {
                throw new ConflictException($"A user with username '{username}' already exists.");
            }

            var emailTaken = await _dbContext.Users
                .AnyAsync(x => x.Id != request.Id && x.Email == email, cancellationToken);

            if (emailTaken)
            {
                throw new ConflictException($"A user with email '{email}' already exists.");
            }

            var requestedRoles = request.Roles
                .Select(role => role.Trim())
                .Distinct()
                .ToList();

            var roles = await _dbContext.Roles
                .Where(role => requestedRoles.Contains(role.Name))
                .ToListAsync(cancellationToken);

            var missingRoles = requestedRoles
                .Except(roles.Select(role => role.Name))
                .ToList();

            if (missingRoles.Count > 0)
            {
                throw new NotFoundException($"Unknown role(s): {string.Join(", ", missingRoles)}.");
            }

            user.Username = username;
            user.FirstName = request.FirstName.Trim();
            user.LastName = request.LastName.Trim();
            user.Email = email;
            user.MobileNumber = string.IsNullOrWhiteSpace(request.MobileNumber)
                ? null
                : request.MobileNumber.Trim();
            user.IsActive = request.IsActive;

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);
            }

            var targetRoleIds = roles.Select(role => role.Id).ToHashSet();

            var rolesToRemove = user.UserRoles
                .Where(userRole => !targetRoleIds.Contains(userRole.RoleId))
                .ToList();

            foreach (var userRole in rolesToRemove)
            {
                user.UserRoles.Remove(userRole);
                _dbContext.UserRoles.Remove(userRole);
            }

            var existingRoleIds = user.UserRoles
                .Select(userRole => userRole.RoleId)
                .ToHashSet();

            foreach (var role in roles.Where(role => !existingRoleIds.Contains(role.Id)))
            {
                user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, Role = role });
            }

            await _dbContext.SaveChangesAsync(cancellationToken);

            return ResultDto<UserDto>.Succeeded(
                user.Adapt<UserDto>(_mappingConfig),
                "User updated successfully.");
        }
    }
}
