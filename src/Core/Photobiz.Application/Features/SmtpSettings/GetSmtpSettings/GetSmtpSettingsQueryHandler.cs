using Mapster;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Photobiz.Application.Common.Exceptions;
using Photobiz.Application.Common.Interfaces;
using Photobiz.Application.Features.SmtpSettings.Common;

namespace Photobiz.Application.Features.SmtpSettings.GetSmtpSettings
{
    public class GetSmtpSettingsQueryHandler : IRequestHandler<GetSmtpSettingsQuery, SmtpSettingDto?>
    {
        private readonly IMasterDbContext _masterDbContext;
        private readonly ITenantService _tenantService;
        private readonly TypeAdapterConfig _mappingConfig;

        public GetSmtpSettingsQueryHandler(
            IMasterDbContext masterDbContext,
            ITenantService tenantService,
            TypeAdapterConfig mappingConfig)
        {
            _masterDbContext = masterDbContext;
            _tenantService = tenantService;
            _mappingConfig = mappingConfig;
        }

        public async Task<SmtpSettingDto?> Handle(GetSmtpSettingsQuery request, CancellationToken cancellationToken)
        {
            var tenantKey = _tenantService.GetCurrentTenantKey()
                ?? throw new TenantNotFoundException("No tenant is associated with the current request.");

            var tenant = await _masterDbContext.Tenants
                .SingleOrDefaultAsync(t => t.TenantKey == tenantKey, cancellationToken)
                ?? throw new TenantNotFoundException($"No tenant is registered for key '{tenantKey}'.");

            var smtpSetting = await _masterDbContext.SmtpSettings
                .SingleOrDefaultAsync(s => s.TenantId == tenant.Id, cancellationToken);

            return smtpSetting?.Adapt<SmtpSettingDto>(_mappingConfig);
        }
    }
}
