using System.Text.RegularExpressions;
using FluentValidation;
using FluentValidation.Results;
using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.Tenants.Common;

namespace Photobiz.Application.Features.Tenants.UploadTenantLogo
{
    public class UploadTenantLogoCommandHandler : IRequestHandler<UploadTenantLogoCommand, ResultDto<TenantDetailsDto>>
    {
        /// <summary>
        /// A logo never needs to be bigger than this to look sharp anywhere it's displayed; capping
        /// it keeps the stored file small, which is good for page-load speed (and so, SEO).
        /// </summary>
        private const int MaxDimensionPixels = 512;

        private readonly IMasterDbContext _masterDbContext;
        private readonly ITenantService _tenantService;
        private readonly IImageProcessor _imageProcessor;
        private readonly ITenantLogoStorage _logoStorage;
        private readonly IPublicUrlProvider _publicUrlProvider;
        private readonly TypeAdapterConfig _mappingConfig;

        public UploadTenantLogoCommandHandler(
            IMasterDbContext masterDbContext,
            ITenantService tenantService,
            IImageProcessor imageProcessor,
            ITenantLogoStorage logoStorage,
            IPublicUrlProvider publicUrlProvider,
            TypeAdapterConfig mappingConfig)
        {
            _masterDbContext = masterDbContext;
            _tenantService = tenantService;
            _imageProcessor = imageProcessor;
            _logoStorage = logoStorage;
            _publicUrlProvider = publicUrlProvider;
            _mappingConfig = mappingConfig;
        }

        public async Task<ResultDto<TenantDetailsDto>> Handle(
            UploadTenantLogoCommand request,
            CancellationToken cancellationToken)
        {
            var tenantKey = _tenantService.GetCurrentTenantKey()
                ?? throw new TenantNotFoundException("No tenant is associated with the current request.");

            var tenant = await _masterDbContext.Tenants
                .SingleOrDefaultAsync(t => t.TenantKey == tenantKey, cancellationToken)
                ?? throw new TenantNotFoundException($"No tenant is registered for key '{tenantKey}'.");

            ProcessedImage processed;
            try
            {
                processed = await _imageProcessor.ConvertToWebPAsync(
                    request.Content, MaxDimensionPixels, cancellationToken);
            }
            catch (UnsupportedImageException ex)
            {
                throw new ValidationException(
                    [new ValidationFailure(nameof(request.Content), ex.Message)]);
            }

            // Descriptive-but-unique: readable/SEO-friendly ("acme-logo-3f1a9c2e.webp") while the
            // random suffix avoids stale-cache collisions with whatever the previous upload was named.
            var slug = Slugify(tenantKey);
            var uniqueSuffix = Guid.NewGuid().ToString("N")[..8];
            var fileName = $"{(slug.Length > 30 ? slug[..30] : slug)}-logo-{uniqueSuffix}.{processed.FileExtension}";
            var baseUrl = _publicUrlProvider.GetBaseUrl();

            var stored = await _logoStorage.SaveAsync(tenantKey, processed.Content, fileName, baseUrl, cancellationToken);

            var previousStoragePath = tenant.SetLogo(stored.PublicUrl, stored.StoragePath);

            await _masterDbContext.SaveChangesAsync(cancellationToken);

            // Only clean up the old file once the new reference is safely persisted — if SaveChanges
            // had thrown, the old file must still be there for the (unchanged) tenant row to point at.
            if (previousStoragePath is not null)
            {
                await _logoStorage.DeleteAsync(previousStoragePath, cancellationToken);
            }

            return ResultDto<TenantDetailsDto>.Succeeded(
                tenant.Adapt<TenantDetailsDto>(_mappingConfig),
                "Logo uploaded successfully.");
        }

        private static string Slugify(string value)
        {
            var slug = Regex.Replace(value.Trim().ToLowerInvariant(), "[^a-z0-9]+", "-").Trim('-');
            return string.IsNullOrEmpty(slug) ? "tenant" : slug;
        }
    }
}
