using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Domain.Entities;
using Photobiz.Infrastructure.Persistence;
using Photobiz.Infrastructure.Persistence.Interceptors;

namespace Photobiz.Infrastructure.Tests.Persistence.Interceptors
{
    public class AuditableEntityInterceptorTests
    {
        private sealed class StubCurrentUser : ICurrentUser
        {
            public string? UserName { get; set; }
        }

        private static PhotobizDbContext CreateContext(ICurrentUser currentUser) =>
            new(new DbContextOptionsBuilder<PhotobizDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .AddInterceptors(new AuditableEntityInterceptor(currentUser))
                .Options);

        private static Client NewClient() => new()
        {
            Id = Guid.NewGuid(),
            Name = "Acme",
            Email = $"{Guid.NewGuid():N}@acme.test"
        };

        [Fact]
        public async Task SaveChanges_OnInsert_StampsCreatedAtAndCreatedBy()
        {
            var currentUser = new StubCurrentUser { UserName = "alice" };
            await using var dbContext = CreateContext(currentUser);
            var client = NewClient();
            dbContext.Clients.Add(client);

            var before = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            Assert.Equal("alice", client.CreatedBy);
            Assert.InRange(client.CreatedAt, before.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
            Assert.Null(client.UpdatedAt);
            Assert.Null(client.UpdatedBy);
        }

        [Fact]
        public async Task SaveChanges_OnUpdate_StampsUpdatedAtAndUpdatedByAndLeavesCreatedIntact()
        {
            var currentUser = new StubCurrentUser { UserName = "alice" };
            await using var dbContext = CreateContext(currentUser);
            var client = NewClient();
            dbContext.Clients.Add(client);
            await dbContext.SaveChangesAsync();
            var createdAt = client.CreatedAt;

            currentUser.UserName = "bob";
            client.Name = "Acme Corp";
            var before = DateTime.UtcNow;
            await dbContext.SaveChangesAsync();

            Assert.Equal("alice", client.CreatedBy);
            Assert.Equal(createdAt, client.CreatedAt);
            Assert.Equal("bob", client.UpdatedBy);
            Assert.NotNull(client.UpdatedAt);
            Assert.InRange(client.UpdatedAt!.Value, before.AddSeconds(-1), DateTime.UtcNow.AddSeconds(1));
        }

        [Fact]
        public async Task SaveChanges_WithNoAuthenticatedUser_LeavesAuthorFieldsNull()
        {
            await using var dbContext = CreateContext(new StubCurrentUser());
            var client = NewClient();
            dbContext.Clients.Add(client);

            await dbContext.SaveChangesAsync();

            Assert.Null(client.CreatedBy);
            Assert.NotEqual(default, client.CreatedAt);
        }

        [Fact]
        public async Task SaveChanges_DoesNotTouchNonAuditableEntities()
        {
            await using var dbContext = CreateContext(new StubCurrentUser { UserName = "alice" });
            dbContext.Roles.Add(new Role { Id = Guid.NewGuid(), Name = "TestRole" });

            var written = await dbContext.SaveChangesAsync();

            Assert.Equal(1, written);
        }

        [Fact]
        public void ApplyAudit_WithNullContext_DoesNothing()
        {
            var interceptor = new AuditableEntityInterceptor(new StubCurrentUser());

            interceptor.ApplyAudit(null);
        }
    }
}
