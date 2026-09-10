using Photobiz.Application.Features.Users.GetUsers;
using Photobiz.Domain.Entities;

namespace Photobiz.Application.Tests.Features.Users.GetUsers
{
    public class GetUsersQueryValidatorTests
    {
        private readonly GetUsersQueryValidator _validator = new();

        [Fact]
        public void Validate_WithDefaults_HasNoErrors()
        {
            var result = _validator.Validate(new GetUsersQuery());

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithKnownRole_HasNoErrors()
        {
            var result = _validator.Validate(new GetUsersQuery(Role: RoleNames.Photographer));

            Assert.True(result.IsValid);
        }

        [Fact]
        public void Validate_WithUnknownRole_HasError()
        {
            var result = _validator.Validate(new GetUsersQuery(Role: "Wizard"));

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetUsersQuery.Role));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void Validate_WithPageNumberBelowOne_HasError(int pageNumber)
        {
            var result = _validator.Validate(new GetUsersQuery(PageNumber: pageNumber));

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetUsersQuery.PageNumber));
        }

        [Theory]
        [InlineData(0)]
        [InlineData(101)]
        public void Validate_WithPageSizeOutOfRange_HasError(int pageSize)
        {
            var result = _validator.Validate(new GetUsersQuery(PageSize: pageSize));

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetUsersQuery.PageSize));
        }

        [Fact]
        public void Validate_WithSearchTextOverMaxLength_HasError()
        {
            var result = _validator.Validate(new GetUsersQuery(SearchText: new string('a', 257)));

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(GetUsersQuery.SearchText));
        }
    }
}
