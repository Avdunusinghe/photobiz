using Photobiz.Application.Features.Users.UpdateUser;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Tests.Features.Users.UpdateUser
{
    public class UpdateUserCommandValidatorTests
    {
        private readonly UpdateUserCommandValidator _validator = new();

        private static UpdateUserCommand Valid() =>
            new(
                Id: Guid.NewGuid(),
                Username: "someone",
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

        [Fact]
        public void Validate_WithEmptyId_HasError()
        {
            var result = _validator.Validate(Valid() with { Id = Guid.Empty });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserCommand.Id));
        }

        [Fact]
        public void Validate_WithNullPassword_HasNoError()
        {
            var result = _validator.Validate(Valid() with { Password = null });

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithShortPassword_HasError()
        {
            var result = _validator.Validate(Valid() with { Password = "short" });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserCommand.Password));
        }

        [Fact]
        public void Validate_WithNoRoles_HasError()
        {
            var result = _validator.Validate(Valid() with { Roles = [] });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserCommand.Roles));
        }

        [Fact]
        public void Validate_WithUnknownRole_HasError()
        {
            var result = _validator.Validate(Valid() with { Roles = ["Wizard"] });

            Assert.Contains(result.Errors, e => e.PropertyName.StartsWith(nameof(UpdateUserCommand.Roles)));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithEmptyLastName_HasError(string lastName)
        {
            var result = _validator.Validate(Valid() with { LastName = lastName });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserCommand.LastName));
        }

        [Fact]
        public void Validate_WithInvalidEmail_HasError()
        {
            var result = _validator.Validate(Valid() with { Email = "not-an-email" });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateUserCommand.Email));
        }

        [Fact]
        public void Validate_WithNullMobileNumber_HasNoError()
        {
            var result = _validator.Validate(Valid() with { MobileNumber = null });

            Assert.True(result.IsValid);
        }
    }
}
