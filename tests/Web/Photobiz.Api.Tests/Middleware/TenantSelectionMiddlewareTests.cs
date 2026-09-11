using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NSubstitute;
using Photobiz.Api.Middleware;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Infrastructure.Persistence;

namespace Photobiz.Api.Tests.Middleware
{
    public class TenantSelectionMiddlewareTests
    {
        private const string PlaceholderConnectionString = "Server=.;Database=Placeholder;Trusted_Connection=True;";

        // A syntactically valid SqlServer connection string is enough: SetConnectionString only
        // ever mutates configuration, it never opens a connection, so these tests never touch a
        // real database.
        private static PhotobizDbContext CreateTenantDbContext() =>
            new(new DbContextOptionsBuilder<PhotobizDbContext>()
                .UseSqlServer(PlaceholderConnectionString)
                .Options);

        private static (TenantSelectionMiddleware Middleware, bool[] NextCalled) CreateMiddleware()
        {
            var nextCalled = new bool[1];
            RequestDelegate next = _ =>
            {
                nextCalled[0] = true;
                return Task.CompletedTask;
            };

            return (new TenantSelectionMiddleware(next), nextCalled);
        }

        private static HttpContext NonAuthRequest() => new DefaultHttpContext
        {
            Request = { Path = "/api/users" }
        };

        private static HttpContext PublicRequest(string host) => new DefaultHttpContext
        {
            Request = { Path = "/api/public/galleries", Host = new HostString(host) }
        };

        // SqlClient rewrites connection strings into its own canonical keyword form
        // ("Server=" -> "Data Source=", etc.), so assert on the database name surviving rather
        // than exact string equality.
        private static void AssertConnectionStringPointsAt(PhotobizDbContext dbContext, string databaseName) =>
            Assert.Contains(databaseName, dbContext.Database.GetConnectionString());

        private static HttpContext LoginRequest(object? body)
        {
            var httpContext = new DefaultHttpContext { Request = { Path = "/api/auth/token" } };

            if (body is not null)
            {
                var json = JsonSerializer.Serialize(body);
                var bytes = Encoding.UTF8.GetBytes(json);
                httpContext.Request.Body = new MemoryStream(bytes);
                httpContext.Request.ContentType = "application/json";
                httpContext.Request.ContentLength = bytes.Length;
            }

            return httpContext;
        }

        [Fact]
        public async Task InvokeAsync_NonAuthPath_WithKnownTenantClaim_SwitchesConnectionStringAndCallsNext()
        {
            var tenantService = Substitute.For<ITenantService>();
            tenantService.GetCurrentTenantKey().Returns("acme");
            tenantService.GetTenantConnectionStringAsync("acme", Arg.Any<CancellationToken>())
                .Returns("Server=.;Database=Tenant_Acme;");

            await using var dbContext = CreateTenantDbContext();
            var (middleware, nextCalled) = CreateMiddleware();

            await middleware.InvokeAsync(NonAuthRequest(), dbContext, tenantService);

            AssertConnectionStringPointsAt(dbContext, "Tenant_Acme");
            Assert.True(nextCalled[0]);
        }

        [Fact]
        public async Task InvokeAsync_NonAuthPath_WithNoTenantClaim_LeavesConnectionStringAndCallsNext()
        {
            var tenantService = Substitute.For<ITenantService>();
            tenantService.GetCurrentTenantKey().Returns((string?)null);

            await using var dbContext = CreateTenantDbContext();
            var (middleware, nextCalled) = CreateMiddleware();

            await middleware.InvokeAsync(NonAuthRequest(), dbContext, tenantService);

            // Anonymous request to a protected endpoint: nothing to resolve, [Authorize] will 401.
            AssertConnectionStringPointsAt(dbContext, "Placeholder");
            Assert.True(nextCalled[0]);
            await tenantService.DidNotReceive().GetTenantConnectionStringAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task InvokeAsync_NonAuthPath_WithUnknownTenantClaim_ThrowsWithoutCallingNext()
        {
            var tenantService = Substitute.For<ITenantService>();
            tenantService.GetCurrentTenantKey().Returns("ghost");
            tenantService.GetTenantConnectionStringAsync("ghost", Arg.Any<CancellationToken>())
                .Returns((string?)null);

            await using var dbContext = CreateTenantDbContext();
            var (middleware, nextCalled) = CreateMiddleware();

            await Assert.ThrowsAsync<TenantNotFoundException>(() =>
                middleware.InvokeAsync(NonAuthRequest(), dbContext, tenantService));

            Assert.False(nextCalled[0]);
        }

        [Fact]
        public async Task InvokeAsync_LoginPath_ReadsTenantKeyFromBody_AndLeavesItReadableForModelBinding()
        {
            var tenantService = Substitute.For<ITenantService>();
            tenantService.GetTenantConnectionStringAsync("acme", Arg.Any<CancellationToken>())
                .Returns("Server=.;Database=Tenant_Acme;");

            await using var dbContext = CreateTenantDbContext();
            var (middleware, nextCalled) = CreateMiddleware();
            var httpContext = LoginRequest(new { tenantKey = "acme", username = "jdoe", password = "secret" });

            await middleware.InvokeAsync(httpContext, dbContext, tenantService);

            AssertConnectionStringPointsAt(dbContext, "Tenant_Acme");
            Assert.True(nextCalled[0]);

            // The body must still be readable, from the start, for the controller's model binder.
            Assert.Equal(0, httpContext.Request.Body.Position);
            using var reader = new StreamReader(httpContext.Request.Body);
            Assert.Contains("acme", await reader.ReadToEndAsync());

            // Login never consults the JWT claim — that's the whole point of this branch.
            tenantService.DidNotReceive().GetCurrentTenantKey();
        }

        [Fact]
        public async Task InvokeAsync_LoginPath_WithUnknownTenantKey_ThrowsWithoutCallingNext()
        {
            var tenantService = Substitute.For<ITenantService>();
            tenantService.GetTenantConnectionStringAsync("ghost", Arg.Any<CancellationToken>())
                .Returns((string?)null);

            await using var dbContext = CreateTenantDbContext();
            var (middleware, nextCalled) = CreateMiddleware();
            var httpContext = LoginRequest(new { tenantKey = "ghost", username = "jdoe", password = "secret" });

            await Assert.ThrowsAsync<TenantNotFoundException>(() =>
                middleware.InvokeAsync(httpContext, dbContext, tenantService));

            Assert.False(nextCalled[0]);
        }

        [Fact]
        public async Task InvokeAsync_LoginPath_WithBlankTenantKey_FallsThroughToModelValidation()
        {
            var tenantService = Substitute.For<ITenantService>();

            await using var dbContext = CreateTenantDbContext();
            var (middleware, nextCalled) = CreateMiddleware();
            var httpContext = LoginRequest(new { tenantKey = "", username = "jdoe", password = "secret" });

            await middleware.InvokeAsync(httpContext, dbContext, tenantService);

            // No tenant to resolve; IssueTokenCommandValidator rejects the blank key downstream.
            AssertConnectionStringPointsAt(dbContext, "Placeholder");
            Assert.True(nextCalled[0]);
            await tenantService.DidNotReceive().GetTenantConnectionStringAsync(Arg.Any<string>(), Arg.Any<CancellationToken>());
        }

        [Fact]
        public async Task InvokeAsync_LoginPath_WithMalformedJson_FallsThroughWithoutThrowing()
        {
            var tenantService = Substitute.For<ITenantService>();

            await using var dbContext = CreateTenantDbContext();
            var (middleware, nextCalled) = CreateMiddleware();
            var httpContext = new DefaultHttpContext { Request = { Path = "/api/auth/token" } };
            var bytes = Encoding.UTF8.GetBytes("{ not valid json");
            httpContext.Request.Body = new MemoryStream(bytes);
            httpContext.Request.ContentType = "application/json";
            httpContext.Request.ContentLength = bytes.Length;

            await middleware.InvokeAsync(httpContext, dbContext, tenantService);

            // Malformed body: leave rejecting it to the controller's own model binding.
            AssertConnectionStringPointsAt(dbContext, "Placeholder");
            Assert.True(nextCalled[0]);
        }

        [Fact]
        public async Task InvokeAsync_LoginPath_WithNonJsonBody_SkipsBodyParsing()
        {
            var tenantService = Substitute.For<ITenantService>();

            await using var dbContext = CreateTenantDbContext();
            var (middleware, nextCalled) = CreateMiddleware();
            // e.g. a CORS preflight OPTIONS to the login endpoint: no JSON content type.
            var httpContext = LoginRequest(body: null);

            await middleware.InvokeAsync(httpContext, dbContext, tenantService);

            AssertConnectionStringPointsAt(dbContext, "Placeholder");
            Assert.True(nextCalled[0]);
        }

        [Fact]
        public async Task InvokeAsync_PublicPath_ResolvesTenantFromHostHeader()
        {
            var tenantService = Substitute.For<ITenantService>();
            tenantService.GetTenantConnectionStringByHostAsync("acme.photobiz.app", Arg.Any<CancellationToken>())
                .Returns("Server=.;Database=Tenant_Acme;");

            await using var dbContext = CreateTenantDbContext();
            var (middleware, nextCalled) = CreateMiddleware();

            await middleware.InvokeAsync(PublicRequest("acme.photobiz.app"), dbContext, tenantService);

            AssertConnectionStringPointsAt(dbContext, "Tenant_Acme");
            Assert.True(nextCalled[0]);

            // Public traffic is anonymous — there's no JWT claim or request body to consult.
            tenantService.DidNotReceive().GetCurrentTenantKey();
        }

        [Fact]
        public async Task InvokeAsync_PublicPath_WithUnresolvableHost_ThrowsWithoutCallingNext()
        {
            var tenantService = Substitute.For<ITenantService>();
            tenantService.GetTenantConnectionStringByHostAsync("nobody.example.com", Arg.Any<CancellationToken>())
                .Returns((string?)null);

            await using var dbContext = CreateTenantDbContext();
            var (middleware, nextCalled) = CreateMiddleware();

            await Assert.ThrowsAsync<TenantNotFoundException>(() =>
                middleware.InvokeAsync(PublicRequest("nobody.example.com"), dbContext, tenantService));

            Assert.False(nextCalled[0]);
        }
    }
}
