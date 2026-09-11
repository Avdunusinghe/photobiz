using Photobiz.Application.Features.Users.GetUsers;
using Photobiz.Application.Tests.Common;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Tests.Features.Users.GetUsers
{
    public class GetUsersQueryHandlerTests
    {
        private readonly InMemoryApplicationDbContext _dbContext = InMemoryApplicationDbContext.Create();
        private readonly GetUsersQueryHandler _handler;

        public GetUsersQueryHandlerTests()
        {
            _handler = new GetUsersQueryHandler(_dbContext, TestMappingConfig.Create());
        }

        [Fact]
        public async Task Handle_ReturnsUsersOrderedByUsernameWithRoles()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            UsersTestData.AddUser(_dbContext, roles, "zoe", RoleNames.Photographer);
            UsersTestData.AddUser(_dbContext, roles, "amy", RoleNames.Admin, RoleNames.Assistant);

            var result = await _handler.Handle(new GetUsersQuery(), CancellationToken.None);

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(["amy", "zoe"], result.Items.Select(x => x.Username));
            Assert.Equal([RoleNames.Admin, RoleNames.Assistant], result.Items[0].Roles);
        }

        [Fact]
        public async Task Handle_IncludesSoftDeletedUsersByDefault()
        {
            // The admin list is the one place deactivated/soft-deleted users must still be visible
            // and manageable (e.g. to reactivate them), so it bypasses the global IsActive filter.
            var roles = UsersTestData.SeedRoles(_dbContext);
            UsersTestData.AddUser(_dbContext, roles, "kept", RoleNames.Admin);
            var removed = UsersTestData.AddUser(_dbContext, roles, "removed", RoleNames.Admin);
            UsersTestData.SoftDelete(_dbContext, removed);

            var result = await _handler.Handle(new GetUsersQuery(), CancellationToken.None);

            Assert.Equal(2, result.TotalCount);
            Assert.Equal(["kept", "removed"], result.Items.Select(x => x.Username));
        }

        [Fact]
        public async Task Handle_WithSearchText_FiltersByUsernameSubstring()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            UsersTestData.AddUser(_dbContext, roles, "alice", RoleNames.Admin);
            UsersTestData.AddUser(_dbContext, roles, "malice", RoleNames.Admin);
            UsersTestData.AddUser(_dbContext, roles, "bob", RoleNames.Admin);

            var result = await _handler.Handle(new GetUsersQuery(SearchText: "lic"), CancellationToken.None);

            Assert.Equal(["alice", "malice"], result.Items.Select(x => x.Username));
        }

        [Fact]
        public async Task Handle_WithSearchText_AlsoMatchesFirstNameLastNameAndEmail()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            UsersTestData.AddUserProfile(
                _dbContext, roles, "u1", "Grace", "Hopper", "grace@navy.mil", null, true, RoleNames.Admin);
            UsersTestData.AddUserProfile(
                _dbContext, roles, "u2", "Alan", "Turing", "alan@bletchley.uk", null, true, RoleNames.Admin);

            var byFirstName = await _handler.Handle(new GetUsersQuery(SearchText: "grac"), CancellationToken.None);
            var byLastName = await _handler.Handle(new GetUsersQuery(SearchText: "turing"), CancellationToken.None);
            var byEmail = await _handler.Handle(new GetUsersQuery(SearchText: "bletchley"), CancellationToken.None);

            Assert.Equal("u1", Assert.Single(byFirstName.Items).Username);
            Assert.Equal("u2", Assert.Single(byLastName.Items).Username);
            Assert.Equal("u2", Assert.Single(byEmail.Items).Username);
        }

        [Fact]
        public async Task Handle_WithIsActiveFilter_ReturnsOnlyMatchingUsers()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            UsersTestData.AddUser(_dbContext, roles, "active-user", isActive: true, RoleNames.Admin);
            UsersTestData.AddUser(_dbContext, roles, "disabled-user", isActive: false, RoleNames.Admin);

            var active = await _handler.Handle(new GetUsersQuery(IsActive: true), CancellationToken.None);
            var disabled = await _handler.Handle(new GetUsersQuery(IsActive: false), CancellationToken.None);
            var all = await _handler.Handle(new GetUsersQuery(), CancellationToken.None);

            Assert.Equal("active-user", Assert.Single(active.Items).Username);
            Assert.Equal("disabled-user", Assert.Single(disabled.Items).Username);
            Assert.Equal(2, all.TotalCount);
        }

        [Fact]
        public async Task Handle_ProjectsProfileFields()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            UsersTestData.AddUserProfile(
                _dbContext, roles, "kate", "Kate", "Bishop", "kate@example.com", "555-7", true, RoleNames.Assistant);

            var result = await _handler.Handle(new GetUsersQuery(), CancellationToken.None);

            var dto = Assert.Single(result.Items);
            Assert.Equal("Kate", dto.FirstName);
            Assert.Equal("Bishop", dto.LastName);
            Assert.Equal("kate@example.com", dto.Email);
            Assert.Equal("555-7", dto.MobileNumber);
            Assert.True(dto.IsActive);
        }

        [Fact]
        public async Task Handle_WithRoleFilter_ReturnsOnlyUsersInThatRole()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            UsersTestData.AddUser(_dbContext, roles, "admin-user", RoleNames.Admin);
            UsersTestData.AddUser(_dbContext, roles, "photographer-user", RoleNames.Photographer);

            var result = await _handler.Handle(new GetUsersQuery(Role: RoleNames.Photographer), CancellationToken.None);

            Assert.Single(result.Items);
            Assert.Equal("photographer-user", result.Items[0].Username);
        }

        [Fact]
        public async Task Handle_AppliesPagination()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            for (var i = 1; i <= 25; i++)
            {
                UsersTestData.AddUser(_dbContext, roles, $"user-{i:D2}", RoleNames.Assistant);
            }

            var result = await _handler.Handle(new GetUsersQuery(PageNumber: 2, PageSize: 10), CancellationToken.None);

            Assert.Equal(25, result.TotalCount);
            Assert.Equal(3, result.TotalPages);
            Assert.Equal(10, result.Items.Count);
            Assert.Equal("user-11", result.Items[0].Username);
            Assert.True(result.HasPreviousPage);
            Assert.True(result.HasNextPage);
        }

        [Fact]
        public async Task Handle_WithNoMatches_ReturnsEmptyPagedResult()
        {
            UsersTestData.SeedRoles(_dbContext);

            var result = await _handler.Handle(new GetUsersQuery(SearchText: "nobody"), CancellationToken.None);

            Assert.Empty(result.Items);
            Assert.Equal(0, result.TotalCount);
        }

        [Fact]
        public async Task Handle_NormalizesOutOfRangePagingArguments()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            UsersTestData.AddUser(_dbContext, roles, "only-user", RoleNames.Admin);

            var result = await _handler.Handle(new GetUsersQuery(PageNumber: 0, PageSize: 0), CancellationToken.None);

            Assert.Equal(1, result.PageNumber);
            Assert.Equal(20, result.PageSize);
            Assert.Single(result.Items);
        }
    }
}
