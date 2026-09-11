using Photobiz.Application.Features.Public.GetTenantDiagnostics;
using Photobiz.Application.Tests.Common;
using Photobiz.Application.Tests.Features.Users;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Tests.Features.Public.GetTenantDiagnostics
{
    public class GetTenantDiagnosticsQueryHandlerTests
    {
        private readonly InMemoryApplicationDbContext _dbContext = InMemoryApplicationDbContext.Create();
        private readonly GetTenantDiagnosticsQueryHandler _handler;

        public GetTenantDiagnosticsQueryHandlerTests()
        {
            _handler = new GetTenantDiagnosticsQueryHandler(_dbContext);
        }

        [Fact]
        public async Task Handle_WithNoUsers_ReturnsZeroCountAndEmptyList()
        {
            var result = await _handler.Handle(new GetTenantDiagnosticsQuery(), CancellationToken.None);

            Assert.Equal(0, result.UserCount);
            Assert.Empty(result.SampleUsernames);
        }

        [Fact]
        public async Task Handle_ReturnsTotalCountAndUpToFiveUsernamesOrderedByUsername()
        {
            var roles = UsersTestData.SeedRoles(_dbContext);
            for (var i = 1; i <= 7; i++)
            {
                UsersTestData.AddUser(_dbContext, roles, $"user-{i:D2}", RoleNames.Admin);
            }

            var result = await _handler.Handle(new GetTenantDiagnosticsQuery(), CancellationToken.None);

            Assert.Equal(7, result.UserCount);
            Assert.Equal(
                ["user-01", "user-02", "user-03", "user-04", "user-05"],
                result.SampleUsernames);
        }

        [Fact]
        public async Task Handle_OnlyReflectsUsersInTheCurrentlyResolvedTenantDbContext()
        {
            // A different InMemoryApplicationDbContext instance stands in for "a different
            // tenant's database" — the whole point of tenant resolution is that only one of these
            // is ever wired up as the ambient IApplicationDbContext for a given request.
            await using var otherTenantDbContext = InMemoryApplicationDbContext.Create();
            var otherTenantRoles = UsersTestData.SeedRoles(otherTenantDbContext);
            UsersTestData.AddUser(otherTenantDbContext, otherTenantRoles, "someone-elses-user", RoleNames.Admin);

            var result = await _handler.Handle(new GetTenantDiagnosticsQuery(), CancellationToken.None);

            Assert.Equal(0, result.UserCount);
        }
    }
}
