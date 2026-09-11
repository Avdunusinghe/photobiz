using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using NSubstitute;
using Photobiz.Api.Services;
using Photobiz.Application.Common.Constants;
using Photobiz.Application.Common.Settings;
using Photobiz.Domain.Entities;
using Photobiz.Infrastructure.Persistence;

namespace Photobiz.Api.Tests.Services
{
    public class TenantServiceTests
    {
        private const string BaseDomain = "photobiz.app";

        private static MasterDbContext CreateMasterDbContext() =>
            new(new DbContextOptionsBuilder<MasterDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options);

        private static TenantService CreateService(MasterDbContext dbContext, HttpContext? httpContext = null) =>
            new(AccessorFor(httpContext), dbContext, Options.Create(new TenancySettings { BaseDomain = BaseDomain }));

        private static IHttpContextAccessor AccessorFor(HttpContext? httpContext)
        {
            var accessor = Substitute.For<IHttpContextAccessor>();
            accessor.HttpContext.Returns(httpContext);
            return accessor;
        }

        private static HttpContext AuthenticatedContext(string tenantKey)
        {
            var identity = new ClaimsIdentity(
                [new Claim(TenantClaimTypes.TenantKey, tenantKey)],
                authenticationType: "TestAuth");

            return new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
        }

        private static Tenant AddTenant(
            MasterDbContext dbContext,
            string tenantKey,
            string connectionString,
            string? customDomain = null,
            bool verifyCustomDomain = false)
        {
            var tenant = Tenant.Create(
                tenantKey, "Acme", connectionString,
                "owner@acme.test", "Ada", "Lovelace", "555", "1 Street", "London", "UK",
                "pro", "monthly", Guid.NewGuid());

            if (customDomain is not null)
            {
                tenant.RequestCustomDomain(customDomain);
                if (verifyCustomDomain)
                {
                    tenant.MarkCustomDomainVerified();
                }
            }

            dbContext.Tenants.Add(tenant);
            dbContext.SaveChanges();
            return tenant;
        }

        [Fact]
        public void GetCurrentTenantKey_WithNoHttpContext_ReturnsNull()
        {
            using var dbContext = CreateMasterDbContext();

            Assert.Null(CreateService(dbContext).GetCurrentTenantKey());
        }

        [Fact]
        public void GetCurrentTenantKey_WithAnonymousRequest_ReturnsNull()
        {
            using var dbContext = CreateMasterDbContext();

            Assert.Null(CreateService(dbContext, new DefaultHttpContext()).GetCurrentTenantKey());
        }

        [Fact]
        public void GetCurrentTenantKey_WithAuthenticatedRequest_ReadsTheTenantClaim()
        {
            using var dbContext = CreateMasterDbContext();

            Assert.Equal("acme", CreateService(dbContext, AuthenticatedContext("acme")).GetCurrentTenantKey());
        }

        [Fact]
        public async Task GetTenantConnectionStringAsync_WithKnownTenant_ReturnsItsConnectionString()
        {
            using var dbContext = CreateMasterDbContext();
            AddTenant(dbContext, "acme", "Server=.;Database=Tenant_Acme;");

            var connectionString = await CreateService(dbContext).GetTenantConnectionStringAsync("acme");

            Assert.Equal("Server=.;Database=Tenant_Acme;", connectionString);
        }

        [Fact]
        public async Task GetTenantConnectionStringAsync_WithUnknownTenant_ReturnsNull()
        {
            using var dbContext = CreateMasterDbContext();

            Assert.Null(await CreateService(dbContext).GetTenantConnectionStringAsync("does-not-exist"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public async Task GetTenantConnectionStringAsync_WithBlankKey_ReturnsNullWithoutQuerying(string tenantKey)
        {
            using var dbContext = CreateMasterDbContext();

            Assert.Null(await CreateService(dbContext).GetTenantConnectionStringAsync(tenantKey));
        }

        [Fact]
        public async Task GetTenantConnectionStringByHostAsync_WithSubdomainOfBaseDomain_ResolvesTheTenantKey()
        {
            using var dbContext = CreateMasterDbContext();
            AddTenant(dbContext, "acme", "Server=.;Database=Tenant_Acme;");

            var connectionString = await CreateService(dbContext)
                .GetTenantConnectionStringByHostAsync($"acme.{BaseDomain}");

            Assert.Equal("Server=.;Database=Tenant_Acme;", connectionString);
        }

        [Fact]
        public async Task GetTenantConnectionStringByHostAsync_WithVerifiedCustomDomain_ResolvesIt()
        {
            using var dbContext = CreateMasterDbContext();
            AddTenant(
                dbContext, "acme", "Server=.;Database=Tenant_Acme;",
                customDomain: "www.acmestudio.test", verifyCustomDomain: true);

            var connectionString = await CreateService(dbContext)
                .GetTenantConnectionStringByHostAsync("www.acmestudio.test");

            Assert.Equal("Server=.;Database=Tenant_Acme;", connectionString);
        }

        [Fact]
        public async Task GetTenantConnectionStringByHostAsync_WithUnverifiedCustomDomain_DoesNotResolve()
        {
            using var dbContext = CreateMasterDbContext();
            AddTenant(
                dbContext, "acme", "Server=.;Database=Tenant_Acme;",
                customDomain: "www.acmestudio.test", verifyCustomDomain: false);

            // Not a subdomain of BaseDomain either, so nothing should resolve.
            Assert.Null(await CreateService(dbContext).GetTenantConnectionStringByHostAsync("www.acmestudio.test"));
        }

        [Fact]
        public async Task GetTenantConnectionStringByHostAsync_IsCaseInsensitive()
        {
            using var dbContext = CreateMasterDbContext();
            AddTenant(
                dbContext, "acme", "Server=.;Database=Tenant_Acme;",
                customDomain: "www.acmestudio.test", verifyCustomDomain: true);

            Assert.Equal(
                "Server=.;Database=Tenant_Acme;",
                await CreateService(dbContext).GetTenantConnectionStringByHostAsync("WWW.ACMESTUDIO.TEST"));
        }

        [Fact]
        public async Task GetTenantConnectionStringByHostAsync_WithUnrelatedHost_ReturnsNull()
        {
            using var dbContext = CreateMasterDbContext();
            AddTenant(dbContext, "acme", "Server=.;Database=Tenant_Acme;");

            Assert.Null(await CreateService(dbContext).GetTenantConnectionStringByHostAsync("example.com"));
        }

        [Fact]
        public async Task GetTenantConnectionStringByHostAsync_WithBareBaseDomain_ReturnsNull()
        {
            using var dbContext = CreateMasterDbContext();

            // No subdomain label at all — must not resolve to an empty tenant key.
            Assert.Null(await CreateService(dbContext).GetTenantConnectionStringByHostAsync(BaseDomain));
        }
    }
}
