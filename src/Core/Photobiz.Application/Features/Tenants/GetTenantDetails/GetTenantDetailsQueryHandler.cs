using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Features.Tenants.Common;

namespace Photobiz.Application.Features.Tenants.GetTenantDetails
{
    public class GetTenantDetailsQueryHandler : IRequestHandler<GetTenantDetailsQuery, TenantDetailsDto>
    {
        private readonly IMasterDbContext _masterDbContext;
        private readonly ITenantService _tenantService;
        private readonly TypeAdapterConfig _mappingConfig;

        public GetTenantDetailsQueryHandler(
            IMasterDbContext masterDbContext,
            ITenantService tenantService,
            TypeAdapterConfig mappingConfig)
        {
            _masterDbContext = masterDbContext;
            _tenantService = tenantService;
            _mappingConfig = mappingConfig;
        }

        public async Task<TenantDetailsDto> Handle(GetTenantDetailsQuery request, CancellationToken cancellationToken)
        {
            var tenantKey = _tenantService.GetCurrentTenantKey()
                ?? throw new TenantNotFoundException("No tenant is associated with the current request.");

            var tenant = await _masterDbContext.Tenants
                .SingleOrDefaultAsync(t => t.TenantKey == tenantKey, cancellationToken)
                ?? throw new TenantNotFoundException($"No tenant is registered for key '{tenantKey}'.");

            return tenant.Adapt<TenantDetailsDto>(_mappingConfig);
        }
    }
}
