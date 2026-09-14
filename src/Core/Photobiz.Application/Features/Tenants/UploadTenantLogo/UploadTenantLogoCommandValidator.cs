using FluentValidation;

namespace Photobiz.Application.Features.Tenants.UploadTenantLogo
{
    public class UploadTenantLogoCommandValidator : AbstractValidator<UploadTenantLogoCommand>
    {
        /// <summary>Generous enough for any real logo while keeping the request body sane; the stored file ends up far smaller once re-encoded.</summary>
        public const long MaxUploadSizeBytes = 5 * 1024 * 1024;

        public static readonly string[] AllowedContentTypes =
        [
            "image/png",
            "image/jpeg",
            "image/webp",
            "image/gif",
        ];

        public UploadTenantLogoCommandValidator()
        {
            RuleFor(x => x.FileName)
                .NotEmpty();

            RuleFor(x => x.Length)
                .GreaterThan(0).WithMessage("The file is empty.")
                .LessThanOrEqualTo(MaxUploadSizeBytes)
                .WithMessage($"Logo must be {MaxUploadSizeBytes / (1024 * 1024)} MB or smaller.");

            RuleFor(x => x.ContentType)
                .Must(contentType => AllowedContentTypes.Contains(contentType?.ToLowerInvariant()))
                .WithMessage($"Logo must be one of: {string.Join(", ", AllowedContentTypes)}.");
        }
    }
}
