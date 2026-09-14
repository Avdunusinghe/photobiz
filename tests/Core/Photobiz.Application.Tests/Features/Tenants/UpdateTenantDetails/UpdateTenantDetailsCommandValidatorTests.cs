using Photobiz.Application.Features.Tenants.UpdateTenantDetails;

namespace Photobiz.Application.Tests.Features.Tenants.UpdateTenantDetails
{
    public class UpdateTenantDetailsCommandValidatorTests
    {
        private readonly UpdateTenantDetailsCommandValidator _validator = new();

        private static UpdateTenantDetailsCommand Valid() => new(
            Name: "Acme Studio",
            CustomerEmail: "owner@acme.test",
            CustomerFirstName: "Ada",
            CustomerLastName: "Lovelace",
            Phone: "+1 555 0100",
            Address: "1 Street",
            City: "London",
            Country: "UK");

        [Fact]
        public void Validate_WithValidCommand_HasNoErrors()
        {
            var result = _validator.Validate(Valid());

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithEmptyName_HasError(string name)
        {
            var result = _validator.Validate(Valid() with { Name = name });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTenantDetailsCommand.Name));
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-an-email")]
        public void Validate_WithInvalidCustomerEmail_HasError(string customerEmail)
        {
            var result = _validator.Validate(Valid() with { CustomerEmail = customerEmail });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTenantDetailsCommand.CustomerEmail));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithEmptyPhone_HasError(string phone)
        {
            var result = _validator.Validate(Valid() with { Phone = phone });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTenantDetailsCommand.Phone));
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithEmptyCountry_HasError(string country)
        {
            var result = _validator.Validate(Valid() with { Country = country });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(UpdateTenantDetailsCommand.Country));
        }
    }
}
