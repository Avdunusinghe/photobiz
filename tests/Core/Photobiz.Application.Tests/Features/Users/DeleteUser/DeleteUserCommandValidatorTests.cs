using Photobiz.Application.Features.Users.DeleteUser;

namespace Photobiz.Application.Tests.Features.Users.DeleteUser
{
    public class DeleteUserCommandValidatorTests
    {
        private readonly DeleteUserCommandValidator _validator = new();

        [Fact]
        public void Validate_WithNonEmptyId_HasNoErrors()
        {
            var result = _validator.Validate(new DeleteUserCommand(Guid.NewGuid()));

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithEmptyId_HasError()
        {
            var result = _validator.Validate(new DeleteUserCommand(Guid.Empty));

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(DeleteUserCommand.Id));
        }
    }
}
