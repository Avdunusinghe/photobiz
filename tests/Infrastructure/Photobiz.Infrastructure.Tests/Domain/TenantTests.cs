using Photobiz.Domain.Entities;

namespace Photobiz.Infrastructure.Tests.Domain
{
    public class TenantTests
    {
        private static Tenant CreateTenant(string tenantKey = "acme") =>
            Tenant.Create(
                tenantKey: tenantKey,
                name: "Acme Studio",
                connectionString: "Server=.;Database=Tenant_Acme;",
                customerEmail: "owner@acme.test",
                customerFirstName: "Ada",
                customerLastName: "Lovelace",
                phone: "+1 555 0100",
                address: "1 Analytical Engine Way",
                city: "London",
                country: "UK",
                planCode: "pro",
                billingCycle: "monthly",
                orderId: Guid.NewGuid());

        [Fact]
        public void Create_GrantsAccessImmediately()
        {
            var tenant = CreateTenant();

            Assert.NotEqual(Guid.Empty, tenant.Id);
            Assert.Equal("acme", tenant.TenantKey);
            Assert.True(tenant.IsSubscribed);
            Assert.Null(tenant.SubscriptionExpiredOn);
        }

        [Fact]
        public void Create_TrimsTheTenantKey()
        {
            var tenant = CreateTenant(tenantKey: "  acme  ");

            Assert.Equal("acme", tenant.TenantKey);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Create_WithBlankTenantKey_Throws(string tenantKey)
        {
            Assert.Throws<ArgumentException>(() => CreateTenant(tenantKey));
        }

        [Fact]
        public void RecordSubscriptionPayment_GrantsAccessAndStoresPaymentDetails()
        {
            var tenant = CreateTenant();
            var expiresOn = DateOnly.FromDateTime(DateTime.UtcNow.AddMonths(1));

            tenant.RecordSubscriptionPayment("pay_123", "sub_456", expiresOn);

            Assert.True(tenant.IsSubscribed);
            Assert.Equal("pay_123", tenant.PayHerePaymentId);
            Assert.Equal("sub_456", tenant.SubscriptionId);
            Assert.Equal(expiresOn, tenant.SubscriptionExpiredOn);
        }

        [Fact]
        public void ExpireSubscription_RevokesAccessButKeepsPaymentHistory()
        {
            var tenant = CreateTenant();
            var expiresOn = DateOnly.FromDateTime(DateTime.UtcNow);
            tenant.RecordSubscriptionPayment("pay_123", "sub_456", expiresOn);

            tenant.ExpireSubscription();

            Assert.False(tenant.IsSubscribed);
            Assert.Equal("pay_123", tenant.PayHerePaymentId);
            Assert.Equal(expiresOn, tenant.SubscriptionExpiredOn);
        }

        [Fact]
        public void UpdateConnectionString_ReplacesIt()
        {
            var tenant = CreateTenant();

            tenant.UpdateConnectionString("Server=new-host;Database=Tenant_Acme;");

            Assert.Equal("Server=new-host;Database=Tenant_Acme;", tenant.ConnectionString);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void UpdateConnectionString_WithBlankValue_Throws(string connectionString)
        {
            var tenant = CreateTenant();

            Assert.Throws<ArgumentException>(() => tenant.UpdateConnectionString(connectionString));
        }

        [Fact]
        public void RequestCustomDomain_StoresItUnverifiedWithAToken()
        {
            var tenant = CreateTenant();

            tenant.RequestCustomDomain("  WWW.AcmeStudio.Test  ");

            Assert.Equal("www.acmestudio.test", tenant.CustomDomain);
            Assert.Null(tenant.CustomDomainVerifiedAt);
            Assert.NotNull(tenant.DomainVerificationToken);
        }

        [Fact]
        public void RequestCustomDomain_CalledAgainWithADifferentDomain_IssuesAFreshToken()
        {
            var tenant = CreateTenant();
            tenant.RequestCustomDomain("www.acmestudio.test");
            var firstToken = tenant.DomainVerificationToken;

            tenant.RequestCustomDomain("www.other-domain.test");

            Assert.Equal("www.other-domain.test", tenant.CustomDomain);
            Assert.NotEqual(firstToken, tenant.DomainVerificationToken);
            Assert.Null(tenant.CustomDomainVerifiedAt);
        }

        [Fact]
        public void RequestCustomDomain_CalledAgainWithTheSameDomain_DoesNotResetAnAlreadyVerifiedDomain()
        {
            var tenant = CreateTenant();
            tenant.RequestCustomDomain("www.acmestudio.test");
            tenant.MarkCustomDomainVerified();

            tenant.RequestCustomDomain("www.acmestudio.test");

            Assert.NotNull(tenant.CustomDomainVerifiedAt);
            Assert.Null(tenant.DomainVerificationToken);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void RequestCustomDomain_WithBlankValue_Throws(string domain)
        {
            var tenant = CreateTenant();

            Assert.Throws<ArgumentException>(() => tenant.RequestCustomDomain(domain));
        }

        [Fact]
        public void MarkCustomDomainVerified_ClearsTheTokenAndStampsVerifiedAt()
        {
            var tenant = CreateTenant();
            tenant.RequestCustomDomain("www.acmestudio.test");

            tenant.MarkCustomDomainVerified();

            Assert.NotNull(tenant.CustomDomainVerifiedAt);
            Assert.Null(tenant.DomainVerificationToken);
            Assert.Equal("www.acmestudio.test", tenant.CustomDomain);
        }

        [Fact]
        public void MarkCustomDomainVerified_WithNoPendingDomain_Throws()
        {
            var tenant = CreateTenant();

            Assert.Throws<InvalidOperationException>(() => tenant.MarkCustomDomainVerified());
        }

        [Fact]
        public void RemoveCustomDomain_ClearsEveryDomainField()
        {
            var tenant = CreateTenant();
            tenant.RequestCustomDomain("www.acmestudio.test");
            tenant.MarkCustomDomainVerified();

            tenant.RemoveCustomDomain();

            Assert.Null(tenant.CustomDomain);
            Assert.Null(tenant.CustomDomainVerifiedAt);
            Assert.Null(tenant.DomainVerificationToken);
        }
    }
}
