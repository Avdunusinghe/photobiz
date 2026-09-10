using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Features.Users.CreateUser;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Tests.Features.Users.CreateUser
{
    public class CreateUserCommandHandlerTests
    {
        private readonly InMemoryApplicationDbContext _dbContext = InMemoryApplicationDbContext.Create();
        private readonly IPasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
        private readonly CreateUserCommandHandler _handler;

        public CreateUserCommandHandlerTests()
        {
            _handler = new CreateUserCommandHandler(_dbContext, _passwordHasher, TestMappingConfig.Create());
        }

        private static CreateUserCommand Command(
            string username = "newuser",
            string password = "sup3r-secret",
            string firstName = "New",
            string lastName = "User",
            string email = "new.user@example.com",
            string? mobileNumber = "+1 555 0100",
            bool isActive = true,
            params string[] roles) =>
            new(username, password, firstName, lastName, email, mobileNumber, isActive,
                roles.Length == 0 ? [RoleNames.Admin] : roles);

        [Fact]
        public async Task Handle_PersistsUserWithHashedPasswordProfileAndRoles()
        {
            UsersTestData.SeedRoles(_dbContext);

            var result = await _handler.Handle(
                Command(
                    username: "  NewUser  ",
                    firstName: "  Ada  ",
                    lastName: "  Lovelace  ",
                    email: "  Ada@Example.com  ",
                    mobileNumber: "  555-0199  ",
                    isActive: false,
                    roles: [RoleNames.Admin, RoleNames.Assistant]),
                CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal("User created successfully.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal("NewUser", result.Data.Username);
            Assert.Equal("Ada", result.Data.FirstName);
            Assert.Equal("Lovelace", result.Data.LastName);
            Assert.Equal("Ada@Example.com", result.Data.Email);
            Assert.Equal("555-0199", result.Data.MobileNumber);
            Assert.False(result.Data.IsActive);
            Assert.Equal([RoleNames.Admin, RoleNames.Assistant], result.Data.Roles);

            var stored = await _dbContext.Users
                .IgnoreQueryFilters()
                .Include(x => x.UserRoles)
                .SingleAsync(x => x.Id == result.Data.Id);

            Assert.NotEqual("sup3r-secret", stored.PasswordHash);
            Assert.Equal(
                PasswordVerificationResult.Success,
                _passwordHasher.VerifyHashedPassword(stored, stored.PasswordHash, "sup3r-secret"));
            Assert.Equal(2, stored.UserRoles.Count);
        }

        [Fact]
        public async Task Handle_WithBlankMobileNumber_StoresNull()
        {
            UsersTestData.SeedRoles(_dbContext);

            var result = await _handler.Handle(
                Command(mobileNumber: "   "),
                CancellationToken.None);

            Assert.Null(result.Data!.MobileNumber);
        }

        [Fact]
        public async Task Handle_WithDuplicateUsername_ThrowsConflictException()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            UsersTestData.AddUser(_dbContext, roles, "existing", RoleNames.Admin);

            await Assert.ThrowsAsync<ConflictException>(() =>
                _handler.Handle(Command(username: "existing"), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithDuplicateEmail_ThrowsConflictException()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            UsersTestData.AddUserProfile(
                _dbContext, roles, "someone", "Some", "One", "taken@example.com", null, true, RoleNames.Admin);

            await Assert.ThrowsAsync<ConflictException>(() =>
                _handler.Handle(Command(username: "fresh", email: "taken@example.com"), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_AllowsReusingUsernameAndEmailOfASoftDeletedUser()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            var removed = UsersTestData.AddUserProfile(
                _dbContext, roles, "reused", "Old", "Owner", "reused@example.com", null, true, RoleNames.Admin);
            UsersTestData.SoftDelete(_dbContext, removed);

            var result = await _handler.Handle(
                Command(username: "reused", email: "reused@example.com"),
                CancellationToken.None);

            Assert.True(result.Success);
            Assert.NotEqual(removed.Id, result.Data!.Id);
        }

        [Fact]
        public async Task Handle_WithUnseededRole_ThrowsNotFoundException()
        {
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(Command(), CancellationToken.None));
        }
    }
}
