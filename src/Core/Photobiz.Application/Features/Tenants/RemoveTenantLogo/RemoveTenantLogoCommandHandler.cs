using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.Tenants.Common;

namespace Photobiz.Application.Features.Tenants.RemoveTenantLogo
{
    public class RemoveTenantLogoCommandHandler : IRequestHandler<RemoveTenantLogoCommand, ResultDto<TenantDetailsDto>>
    {
        private readonly IMasterDbContext _masterDbContext;
        private readonly ITenantService _tenantService;
        private readonly ITenantLogoStorage _logoStorage;
        private readonly TypeAdapterConfig _mappingConfig;

        public RemoveTenantLogoCommandHandler(
            IMasterDbContext masterDbContext,
            ITenantService tenantService,
            ITenantLogoStorage logoStorage,
            TypeAdapterConfig mappingConfig)
        {
            _masterDbContext = masterDbContext;
            _tenantService = tenantService;
            _logoStorage = logoStorage;
            _mappingConfig = mappingConfig;
        }

        public async Task<ResultDto<TenantDetailsDto>> Handle(
            RemoveTenantLogoCommand request,
            CancellationToken cancellationToken)
        {
            var tenantKey = _tenantService.GetCurrentTenantKey()
                ?? throw new TenantNotFoundException("No tenant is associated with the current request.");

            var tenant = await _masterDbContext.Tenants
                .SingleOrDefaultAsync(t => t.TenantKey == tenantKey, cancellationToken)
                ?? throw new TenantNotFoundException($"No tenant is registered for key '{tenantKey}'.");

            var previousStoragePath = tenant.RemoveLogo();

            await _masterDbContext.SaveChangesAsync(cancellationToken);

            if (previousStoragePath is not null)
            {
                await _logoStorage.DeleteAsync(previousStoragePath, cancellationToken);
            }

            return ResultDto<TenantDetailsDto>.Succeeded(
                tenant.Adapt<TenantDetailsDto>(_mappingConfig),
                "Logo removed successfully.");
        }
    }
}
