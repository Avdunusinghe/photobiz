using Photobiz.Application.Features.Auth.IssueToken;

namespace Photobiz.Application.Tests.Features.Auth.IssueToken
{
    public class IssueTokenCommandValidatorTests
    {
        private readonly IssueTokenCommandValidator _validator = new();

        [Fact]
        public void Validate_WithValidUsername_HasNoErrors()
        {
            var result = _validator.Validate(new IssueTokenCommand("someone"));

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithEmptyUsername_HasError(string username)
        {
            var result = _validator.Validate(new IssueTokenCommand(username));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(IssueTokenCommand.Username));
        }

        [Fact]
        public void Validate_WithUsernameOverMaxLength_HasError()
        {
            var username = new string('a', 257);

            var result = _validator.Validate(new IssueTokenCommand(username));

            Assert.False(result.IsValid);
            Assert.Contains(result.Errors, e => e.PropertyName == nameof(IssueTokenCommand.Username));
        }
    }
}
