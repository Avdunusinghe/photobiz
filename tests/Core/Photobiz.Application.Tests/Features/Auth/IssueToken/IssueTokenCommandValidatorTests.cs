using Photobiz.Application.Features.Auth.IssueToken;

namespace Photobiz.Application.Tests.Features.Auth.IssueToken
{
    public class IssueTokenCommandValidatorTests
    {
        private readonly IssueTokenCommandValidator _validator = new();

        [Fact]
        public void Validate_WithValidTenantKeyUsernameAndPassword_HasNoErrors()
        {
            var result = _validator.Validate(new IssueTokenCommand("acme", "someone", "password123"));

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithEmptyTenantKey_HasError(string tenantKey)
        {
            var result = _validator.Validate(new IssueTokenCommand(tenantKey, "someone", "password123"));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(IssueTokenCommand.TenantKey));
        }

        [Fact]
        public void Validate_WithTenantKeyOverMaxLength_HasError()
        {
            var tenantKey = new string('a', 129);

            var result = _validator.Validate(new IssueTokenCommand(tenantKey, "someone", "password123"));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(IssueTokenCommand.TenantKey));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithEmptyUsername_HasError(string username)
        {
            var result = _validator.Validate(new IssueTokenCommand("acme", username, "password123"));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(IssueTokenCommand.Username));
        }

        [Fact]
        public void Validate_WithUsernameOverMaxLength_HasError()
        {
            var username = new string('a', 257);

            var result = _validator.Validate(new IssueTokenCommand("acme", username, "password123"));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(IssueTokenCommand.Username));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithEmptyPassword_HasError(string password)
        {
            var result = _validator.Validate(new IssueTokenCommand("acme", "someone", password));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(IssueTokenCommand.Password));
        }
    }
}
