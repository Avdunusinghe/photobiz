using Photobiz.Application.Features.Tenants.UploadTenantLogo;

namespace Photobiz.Application.Tests.Features.Tenants.UploadTenantLogo
{
    public class UploadTenantLogoCommandValidatorTests
    {
        private readonly UploadTenantLogoCommandValidator _validator = new();

        private static UploadTenantLogoCommand Valid() => new(
            Content: new MemoryStream([1, 2, 3]),
            FileName: "logo.png",
            ContentType: "image/png",
            Length: 3);

        [Fact]
        public void Validate_WithValidCommand_HasNoErrors()
        {
            var result = _validator.Validate(Valid());

            Assert.True(result.IsValid);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Validate_WithEmptyFileName_HasError(string fileName)
        {
            var result = _validator.Validate(Valid() with { FileName = fileName });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(Application.Features.Tenants.UploadTenantLogo.UploadTenantLogoCommand.FileName));
        }

        [Fact]
        public void Validate_WithZeroLength_HasError()
        {
            var result = _validator.Validate(Valid() with { Length = 0 });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(Application.Features.Tenants.UploadTenantLogo.UploadTenantLogoCommand.Length));
        }

        [Fact]
        public void Validate_WithLengthOverTheLimit_HasError()
        {
            var result = _validator.Validate(Valid() with { Length = UploadTenantLogoCommandValidator.MaxUploadSizeBytes + 1 });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(Application.Features.Tenants.UploadTenantLogo.UploadTenantLogoCommand.Length));
        }

        [Fact]
        public void Validate_WithLengthAtTheLimit_HasNoLengthError()
        {
            var result = _validator.Validate(Valid() with { Length = UploadTenantLogoCommandValidator.MaxUploadSizeBytes });

            Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(Application.Features.Tenants.UploadTenantLogo.UploadTenantLogoCommand.Length));
        }

        [Theory]
        [InlineData("application/pdf")]
        [InlineData("text/plain")]
        [InlineData("")]
        public void Validate_WithDisallowedContentType_HasError(string contentType)
        {
            var result = _validator.Validate(Valid() with { ContentType = contentType });

            Assert.Contains(result.Errors, e => e.PropertyName == nameof(Application.Features.Tenants.UploadTenantLogo.UploadTenantLogoCommand.ContentType));
        }

        [Theory]
        [InlineData("image/png")]
        [InlineData("image/jpeg")]
        [InlineData("image/webp")]
        [InlineData("image/gif")]
        [InlineData("IMAGE/PNG")]
        public void Validate_WithAllowedContentType_HasNoError(string contentType)
        {
            var result = _validator.Validate(Valid() with { ContentType = contentType });

            Assert.DoesNotContain(result.Errors, e => e.PropertyName == nameof(Application.Features.Tenants.UploadTenantLogo.UploadTenantLogoCommand.ContentType));
        }
    }
}
