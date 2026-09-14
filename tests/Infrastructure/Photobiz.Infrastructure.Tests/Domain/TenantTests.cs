using Photobiz.Domain.Entities.Master;

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
        public void UpdateProfile_ReplacesNameAndContactDetails()
        {
            var tenant = CreateTenant();

            tenant.UpdateProfile(
                "  Acme Studio Ltd  ",
                "  hello@acmestudio.test  ",
                "  Grace  ",
                "  Hopper  ",
                "  +1 555 0199  ",
                "  221B Baker Street  ",
                "  Manchester  ",
                "  UK  ");

            Assert.Equal("Acme Studio Ltd", tenant.Name);
            Assert.Equal("hello@acmestudio.test", tenant.CustomerEmail);
            Assert.Equal("Grace", tenant.CustomerFirstName);
            Assert.Equal("Hopper", tenant.CustomerLastName);
            Assert.Equal("+1 555 0199", tenant.Phone);
            Assert.Equal("221B Baker Street", tenant.Address);
            Assert.Equal("Manchester", tenant.City);
            Assert.Equal("UK", tenant.Country);
        }

        [Fact]
        public void UpdateProfile_NeverTouchesTheLogo()
        {
            var tenant = CreateTenant();
            tenant.SetLogo("https://cdn.acmestudio.test/logo.webp", "Tenant/acme/Photos/Logo/acme-logo-abc.webp");

            tenant.UpdateProfile(
                "Acme Studio Ltd", "hello@acmestudio.test", "Grace", "Hopper",
                "+1 555 0199", "221B Baker Street", "Manchester", "UK");

            Assert.Equal("https://cdn.acmestudio.test/logo.webp", tenant.LogoUrl);
            Assert.Equal("Tenant/acme/Photos/Logo/acme-logo-abc.webp", tenant.LogoStoragePath);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void UpdateProfile_WithBlankName_Throws(string name)
        {
            var tenant = CreateTenant();

            Assert.Throws<ArgumentException>(() => tenant.UpdateProfile(
                name, "owner@acme.test", "Ada", "Lovelace", "+1 555 0100", "1 Street", "London", "UK"));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void UpdateProfile_WithBlankCustomerEmail_Throws(string customerEmail)
        {
            var tenant = CreateTenant();

            Assert.Throws<ArgumentException>(() => tenant.UpdateProfile(
                "Acme Studio", customerEmail, "Ada", "Lovelace", "+1 555 0100", "1 Street", "London", "UK"));
        }

        [Fact]
        public void SetLogo_StoresUrlAndStoragePath()
        {
            var tenant = CreateTenant();

            tenant.SetLogo("https://cdn.acmestudio.test/logo.webp", "Tenant/acme/Photos/Logo/acme-logo-abc.webp");

            Assert.Equal("https://cdn.acmestudio.test/logo.webp", tenant.LogoUrl);
            Assert.Equal("Tenant/acme/Photos/Logo/acme-logo-abc.webp", tenant.LogoStoragePath);
        }

        [Fact]
        public void SetLogo_WhenNoPreviousLogo_ReturnsNull()
        {
            var tenant = CreateTenant();

            var previous = tenant.SetLogo("https://cdn.acmestudio.test/logo.webp", "Tenant/acme/Photos/Logo/a.webp");

            Assert.Null(previous);
        }

        [Fact]
        public void SetLogo_CalledAgain_ReturnsThePreviousStoragePathForCleanup()
        {
            var tenant = CreateTenant();
            tenant.SetLogo("https://cdn.acmestudio.test/logo-1.webp", "Tenant/acme/Photos/Logo/logo-1.webp");

            var previous = tenant.SetLogo("https://cdn.acmestudio.test/logo-2.webp", "Tenant/acme/Photos/Logo/logo-2.webp");

            Assert.Equal("Tenant/acme/Photos/Logo/logo-1.webp", previous);
            Assert.Equal("https://cdn.acmestudio.test/logo-2.webp", tenant.LogoUrl);
            Assert.Equal("Tenant/acme/Photos/Logo/logo-2.webp", tenant.LogoStoragePath);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void SetLogo_WithBlankUrl_Throws(string logoUrl)
        {
            var tenant = CreateTenant();

            Assert.Throws<ArgumentException>(() => tenant.SetLogo(logoUrl, "Tenant/acme/Photos/Logo/a.webp"));
        }

        [Fact]
        public void RemoveLogo_ClearsUrlAndStoragePath()
        {
            var tenant = CreateTenant();
            tenant.SetLogo("https://cdn.acmestudio.test/logo.webp", "Tenant/acme/Photos/Logo/a.webp");

            var previous = tenant.RemoveLogo();

            Assert.Equal("Tenant/acme/Photos/Logo/a.webp", previous);
            Assert.Null(tenant.LogoUrl);
            Assert.Null(tenant.LogoStoragePath);
        }

        [Fact]
        public void RemoveLogo_WhenNoLogoIsSet_ReturnsNullAndIsANoOp()
        {
            var tenant = CreateTenant();

            var previous = tenant.RemoveLogo();

            Assert.Null(previous);
            Assert.Null(tenant.LogoUrl);
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
