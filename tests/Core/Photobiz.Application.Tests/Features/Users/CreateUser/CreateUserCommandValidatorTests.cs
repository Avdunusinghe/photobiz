using Photobiz.Application.Features.Users.CreateUser;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Tests.Features.Users.CreateUser
{
    public class CreateUserCommandValidatorTests
    {
        private readonly CreateUserCommandValidator _validator = new();

        private static CreateUserCommand Valid() =>
            new(
                Username: "someone",
                Password: "password123",
                FirstName: "Some",
                LastName: "One",
                Email: "some.one@example.com",
                MobileNumber: "+1 555 0100",
                IsActive: true,
                Roles: [RoleNames.Admin]);

        [Fact]
        public void Validate_WithValidCommand_HasNoErrors()
        {
            var result = _validator.Validate(Valid());

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithEmptyUsername_HasError(string username)
        {
            var result = _validator.Validate(Valid() with { Username = username });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.Username));
        }

        [Fact]
        public void Validate_WithShortPassword_HasError()
        {
            var result = _validator.Validate(Valid() with { Password = "short" });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.Password));
        }

        [Fact]
        public void Validate_WithNoRoles_HasError()
        {
            var result = _validator.Validate(Valid() with { Roles = [] });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.Roles));
        }

        [Fact]
        public void Validate_WithUnknownRole_HasError()
        {
            var result = _validator.Validate(Valid() with { Roles = ["Wizard"] });

            Assert.Contains(result.Errors, e => e.PropertyName.StartsWith(nameof(CreateUserCommand.Roles)));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithEmptyFirstName_HasError(string firstName)
        {
            var result = _validator.Validate(Valid() with { FirstName = firstName });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.FirstName));
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-an-email")]
        public void Validate_WithInvalidEmail_HasError(string email)
        {
            var result = _validator.Validate(Valid() with { Email = email });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.Email));
        }

        [Fact]
        public void Validate_WithNullMobileNumber_HasNoError()
        {
            var result = _validator.Validate(Valid() with { MobileNumber = null });

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithMalformedMobileNumber_HasError()
        {
            var result = _validator.Validate(Valid() with { MobileNumber = "abc-123" });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateUserCommand.MobileNumber));
        }
    }
}
