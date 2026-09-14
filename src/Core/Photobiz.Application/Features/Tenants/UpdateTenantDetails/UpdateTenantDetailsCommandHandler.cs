using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.Tenants.Common;

namespace Photobiz.Application.Features.Tenants.UpdateTenantDetails
{
    public class UpdateTenantDetailsCommandHandler
        : IRequestHandler<UpdateTenantDetailsCommand, ResultDto<TenantDetailsDto>>
    {
        private readonly IMasterDbContext _masterDbContext;
        private readonly ITenantService _tenantService;
        private readonly TypeAdapterConfig _mappingConfig;

        public UpdateTenantDetailsCommandHandler(
            IMasterDbContext masterDbContext,
            ITenantService tenantService,
            TypeAdapterConfig mappingConfig)
        {
            _masterDbContext = masterDbContext;
            _tenantService = tenantService;
            _mappingConfig = mappingConfig;
        }

        public async Task<ResultDto<TenantDetailsDto>> Handle(
            UpdateTenantDetailsCommand request,
            CancellationToken cancellationToken)
        {
            var tenantKey = _tenantService.GetCurrentTenantKey()
                ?? throw new TenantNotFoundException("No tenant is associated with the current request.");

            var tenant = await _masterDbContext.Tenants
                .SingleOrDefaultAsync(t => t.TenantKey == tenantKey, cancellationToken)
                ?? throw new TenantNotFoundException($"No tenant is registered for key '{tenantKey}'.");

            tenant.UpdateProfile(
                request.Name,
                request.CustomerEmail,
                request.CustomerFirstName,
                request.CustomerLastName,
                request.Phone,
                request.Address,
                request.City,
                request.Country);

            await _masterDbContext.SaveChangesAsync(cancellationToken);

            return ResultDto<TenantDetailsDto>.Succeeded(
                tenant.Adapt<TenantDetailsDto>(_mappingConfig),
                "Tenant details updated successfully.");
        }
    }
}
