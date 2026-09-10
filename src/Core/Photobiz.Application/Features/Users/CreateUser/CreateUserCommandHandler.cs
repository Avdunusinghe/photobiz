using Mapster;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.Users.Common;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Features.Users.CreateUser
{
    public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, ResultDto<UserDto>>
    {
        private readonly IApplicationDbContext _dbContext;
        private readonly IPasswordHasher<User> _passwordHasher;
        private readonly TypeAdapterConfig _mappingConfig;

        public CreateUserCommandHandler(
            IApplicationDbContext dbContext,
            IPasswordHasher<User> passwordHasher,
            TypeAdapterConfig mappingConfig)
        {
            _dbContext = dbContext;
            _passwordHasher = passwordHasher;
            _mappingConfig = mappingConfig;
        }

        public async Task<ResultDto<UserDto>> Handle(CreateUserCommand request, CancellationToken cancellationToken)
        {
            var username = request.Username.Trim();
            var email = request.Email.Trim();

            var usernameTaken = await _dbContext.Users
                .AnyAsync(user => user.Username == username, cancellationToken);

            if (usernameTaken)
            {
                throw new ConflictException($"A user with username '{username}' already exists.");
            }

            var emailTaken = await _dbContext.Users
                .AnyAsync(user => user.Email == email, cancellationToken);

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

            var user = new User
            {
                Id = Guid.NewGuid(),
                Username = username,
                PasswordHash = string.Empty,
                FirstName = request.FirstName.Trim(),
                LastName = request.LastName.Trim(),
                Email = email,
                MobileNumber = string.IsNullOrWhiteSpace(request.MobileNumber)
                    ? null
                    : request.MobileNumber.Trim(),
                IsActive = request.IsActive
                // CreatedAt / CreatedBy are stamped by AuditableEntityInterceptor on save.
            };
            user.PasswordHash = _passwordHasher.HashPassword(user, request.Password);

            foreach (var role in roles)
            {
                user.UserRoles.Add(new UserRole { UserId = user.Id, RoleId = role.Id, Role = role });
            }

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);

            return ResultDto<UserDto>.Succeeded(
                user.Adapt<UserDto>(_mappingConfig),
                "User created successfully.");
        }
    }
}
