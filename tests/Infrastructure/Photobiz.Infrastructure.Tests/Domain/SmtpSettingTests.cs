using Photobiz.Domain.Entities.Master;

namespace Photobiz.Infrastructure.Tests.Domain
{
    public class SmtpSettingTests
    {
        private static SmtpSetting CreateSetting(Guid? tenantId = null) =>
            SmtpSetting.Create(
                tenantId: tenantId ?? Guid.NewGuid(),
                host: "smtp.acmestudio.test",
                port: 587,
                username: "no-reply@acmestudio.test",
                password: "s3cret",
                enableSsl: true,
                fromEmail: "no-reply@acmestudio.test",
                fromName: "Acme Studio");

        [Fact]
        public void Create_IsEnabledByDefault()
        {
            var setting = CreateSetting();

            Assert.True(setting.IsEnabled);
            Assert.NotEqual(Guid.Empty, setting.Id);
        }

        [Fact]
        public void Create_TrimsHostUsernameAndFromFields()
        {
            var setting = SmtpSetting.Create(
                Guid.NewGuid(), "  smtp.acmestudio.test  ", 587,
                "  no-reply@acmestudio.test  ", "s3cret", true,
                "  no-reply@acmestudio.test  ", "  Acme Studio  ");

            Assert.Equal("smtp.acmestudio.test", setting.Host);
            Assert.Equal("no-reply@acmestudio.test", setting.Username);
            Assert.Equal("no-reply@acmestudio.test", setting.FromEmail);
            Assert.Equal("Acme Studio", setting.FromName);
        }

        [Fact]
        public void Create_WithBlankFromName_StoresNull()
        {
            var setting = SmtpSetting.Create(
                Guid.NewGuid(), "smtp.acmestudio.test", 587,
                "no-reply@acmestudio.test", "s3cret", true,
                "no-reply@acmestudio.test", fromName: "   ");

            Assert.Null(setting.FromName);
        }

        [Fact]
        public void Create_WithEmptyTenantId_Throws()
        {
            Assert.Throws<ArgumentException>(() => CreateSetting(Guid.Empty));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-25)]
        [InlineData(65536)]
        public void UpdateConnection_WithInvalidPort_Throws(int port)
        {
            var setting = CreateSetting();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                setting.UpdateConnection("smtp.acmestudio.test", port, "user", "pass", true));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void UpdateConnection_WithBlankHost_Throws(string host)
        {
            var setting = CreateSetting();

            Assert.Throws<ArgumentException>(() => setting.UpdateConnection(host, 587, "user", "pass", true));
        }

        [Fact]
        public void UpdateConnection_ReplacesServerDetails()
        {
            var setting = CreateSetting();

            setting.UpdateConnection("smtp.newhost.test", 465, "new-user", "new-pass", false);

            Assert.Equal("smtp.newhost.test", setting.Host);
            Assert.Equal(465, setting.Port);
            Assert.Equal("new-user", setting.Username);
            Assert.Equal("new-pass", setting.Password);
            Assert.False(setting.EnableSsl);
        }

        [Fact]
        public void UpdateSender_ReplacesFromAddressAndName()
        {
            var setting = CreateSetting();

            setting.UpdateSender("hello@acmestudio.test", "Acme Studio Bookings");

            Assert.Equal("hello@acmestudio.test", setting.FromEmail);
            Assert.Equal("Acme Studio Bookings", setting.FromName);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void UpdateSender_WithBlankFromEmail_Throws(string fromEmail)
        {
            var setting = CreateSetting();

            Assert.Throws<ArgumentException>(() => setting.UpdateSender(fromEmail, "Acme Studio"));
        }

        [Fact]
        public void Disable_ThenEnable_TogglesIsEnabledWithoutLosingConfiguration()
        {
            var setting = CreateSetting();

            setting.Disable();
            Assert.False(setting.IsEnabled);
            Assert.Equal("smtp.acmestudio.test", setting.Host);

            setting.Enable();
            Assert.True(setting.IsEnabled);
        }
    }
}
