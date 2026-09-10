using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Features.Users.DeleteUser;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Tests.Features.Users.DeleteUser
{
    public class DeleteUserCommandHandlerTests
    {
        private readonly InMemoryApplicationDbContext _dbContext = InMemoryApplicationDbContext.Create();
        private readonly DeleteUserCommandHandler _handler;

        public DeleteUserCommandHandlerTests()
        {
            _handler = new DeleteUserCommandHandler(_dbContext);
        }

        [Fact]
        public async Task Handle_SoftDeletesUserAndRevokesAccessButKeepsRoleAssignments()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            var user = UsersTestData.AddUser(_dbContext, roles, "to-delete", RoleNames.Admin, RoleNames.Assistant);

            var result = await _handler.Handle(new DeleteUserCommand(user.Id), CancellationToken.None);

            Assert.True(result.Success);
            Assert.Equal("User deleted successfully.", result.Message);

            // Hidden from normal queries by the global filter.
            Assert.False(await _dbContext.Users.AnyAsync(x => x.Id == user.Id));

            // Row is retained and deactivated.
            var stored = await _dbContext.Users
                .IgnoreQueryFilters()
                .SingleAsync(x => x.Id == user.Id);
            Assert.False(stored.IsActive);

            // Role assignments survive so the user can be restored later.
            Assert.Equal(2, await _dbContext.UserRoles.CountAsync(x => x.UserId == user.Id));
        }

        [Fact]
        public async Task Handle_LeavesOtherUsersUntouched()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            var target = UsersTestData.AddUser(_dbContext, roles, "target", RoleNames.Admin);
            var survivor = UsersTestData.AddUser(_dbContext, roles, "survivor", RoleNames.Photographer);

            await _handler.Handle(new DeleteUserCommand(target.Id), CancellationToken.None);

            Assert.True(await _dbContext.Users.AnyAsync(x => x.Id == survivor.Id));
        }

        [Fact]
        public async Task Handle_WhenAlreadySoftDeleted_ThrowsNotFoundException()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            var user = UsersTestData.AddUser(_dbContext, roles, "gone", RoleNames.Admin);
            await _handler.Handle(new DeleteUserCommand(user.Id), CancellationToken.None);

            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(new DeleteUserCommand(user.Id), CancellationToken.None));
        }

        [Fact]
        public async Task Handle_WithUnknownUserId_ThrowsNotFoundException()
        {
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _handler.Handle(new DeleteUserCommand(Guid.NewGuid()), CancellationToken.None));
        }
    }
}
