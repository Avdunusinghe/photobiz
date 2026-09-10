using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Features.Users.UpdateUser;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Tests.Features.Users.UpdateUser
{
    public class UpdateUserCommandHandlerTests
    {
        private readonly InMemoryApplicationDbContext _dbContext = InMemoryApplicationDbContext.Create();
        private readonly IPasswordHasher<User> _passwordHasher = new PasswordHasher<User>();
        private readonly UpdateUserCommandHandler _handler;

        public UpdateUserCommandHandlerTests()
        {
            _handler = new UpdateUserCommandHandler(_dbContext, _passwordHasher, TestMappingConfig.Create());
        }

        private static UpdateUserCommand Command(
            Guid id,
            string username = "user",
            string firstName = "First",
            string lastName = "Last",
            string email = "user@example.com",
            string? mobileNumber = null,
            bool isActive = true,
            string? password = null,
            params string[] roles) =>
            new(id, username, firstName, lastName, email, mobileNumber, isActive,
                roles.Length == 0 ? [RoleNames.Admin] : roles, password);

        [Fact]
        public async Task Handle_UpdatesUsernameAndReconcilesRoles()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            var user = UsersTestData.AddUser(_dbContext, roles, "old-name", RoleNames.Admin, RoleNames.Assistant);

            var result = await _handler.Handle(
                Command(user.Id, username: "new-name", roles: [RoleNames.Assistant, RoleNames.Photographer]),
                CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal("User updated successfully.", result.Message);
            Assert.NotNull(result.Data);
            Assert.Equal("new-name", result.Data.Username);
            Assert.Equal([RoleNames.Assistant, RoleNames.Photographer], result.Data.Roles);

            var stored = await _dbContext.Users
                .Include(x => x.UserRoles)
                .SingleAsync(x => x.Id == user.Id);

            Assert.Equal("new-name", stored.Username);
            Assert.Equal(2, stored.UserRoles.Count);
        }

        [Fact]
        public async Task Handle_UpdatesProfileFields()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            var user = UsersTestData.AddUser(_dbContext, roles, "user", RoleNames.Admin);

            var result = await _handler.Handle(
                Command(
                    user.Id,
                    firstName: "  Grace  ",
                    lastName: "  Hopper  ",
                    email: "  grace@navy.mil  ",
                    mobileNumber: "  555-2000  ",
                    isActive: false),
                CancellationToken.None);

            Assert.Equal("Grace", result.Data!.FirstName);
            Assert.Equal("Hopper", result.Data.LastName);
            Assert.Equal("grace@navy.mil", result.Data.Email);
            Assert.Equal("555-2000", result.Data.MobileNumber);
            Assert.False(result.Data.IsActive);

            // Deactivating hides the row from the default query, so read past the filter.
            var stored = await _dbContext.Users.IgnoreQueryFilters().SingleAsync(x => x.Id == user.Id);
            Assert.Equal("grace@navy.mil", stored.Email);
            Assert.False(stored.IsActive);
        }

        [Fact]
        public async Task Handle_WithBlankMobileNumber_StoresNull()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            var user = UsersTestData.AddUserProfile(
                _dbContext, roles, "user", "First", "Last", "user@example.com", "555-1", true, RoleNames.Admin);

            var result = await _handler.Handle(
                Command(user.Id, mobileNumber: "   "),
                CancellationToken.None);

            Assert.Null(result.Data!.MobileNumber);
        }

        [Fact]
        public async Task Handle_WithoutPassword_LeavesPasswordHashUnchanged()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            var user = UsersTestData.AddUser(_dbContext, roles, "user", RoleNames.Admin);
            var originalHash = user.PasswordHash;

            await _handler.Handle(Command(user.Id), CancellationToken.None);

            var stored = await _dbContext.Users.SingleAsync(x => x.Id == user.Id);
            Assert.Equal(originalHash, stored.PasswordHash);
        }

        [Fact]
        public async Task Handle_WithPassword_RehashesPassword()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            var user = UsersTestData.AddUser(_dbContext, roles, "user", RoleNames.Admin);

            await _handler.Handle(
                Command(user.Id, password: "brand-new-password"),
                CancellationToken.None);

            var stored = await _dbContext.Users.SingleAsync(x => x.Id == user.Id);
            Assert.Equal(
                PasswordVerificationResult.Success,
                _passwordHasher.VerifyHashedPassword(stored, stored.PasswordHash, "brand-new-password"));
        }

        [Fact]
        public async Task Handle_WithUnknownUserId_ThrowsNotFoundException()
        {
            UsersTestData.SeedRoles(_dbContext);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(Command(Guid.NewGuid()), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithUsernameTakenByAnotherUser_ThrowsConflictException()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            UsersTestData.AddUser(_dbContext, roles, "taken", RoleNames.Admin);
            var user = UsersTestData.AddUser(_dbContext, roles, "mine", RoleNames.Admin);

            await Assert.ThrowsAsync<ConflictException>(() =>
                _handler.Handle(Command(user.Id, username: "taken"), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithEmailTakenByAnotherUser_ThrowsConflictException()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            UsersTestData.AddUserProfile(
                _dbContext, roles, "other", "Other", "Person", "shared@example.com", null, true, RoleNames.Admin);
            var user = UsersTestData.AddUser(_dbContext, roles, "mine", RoleNames.Admin);

            await Assert.ThrowsAsync<ConflictException>(() =>
                _handler.Handle(Command(user.Id, username: "mine", email: "shared@example.com"), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_KeepingSameUsername_DoesNotThrowConflict()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            var user = UsersTestData.AddUserProfile(
                _dbContext, roles, "same", "First", "Last", "same@example.com", null, true, RoleNames.Admin);

            var result = await _handler.Handle(
                Command(user.Id, username: "same", email: "same@example.com"),
                CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal("same", result.Data!.Username);
        }
    }
}
