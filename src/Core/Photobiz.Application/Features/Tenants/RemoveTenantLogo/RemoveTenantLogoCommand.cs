using MediatR;
using Photobiz.Application.Common.Models;
using Photobiz.Application.Features.Tenants.Common;

namespace Photobiz.Application.Features.Tenants.RemoveTenantLogo
{
    /// <summary>Removes the calling user's own tenant's logo — always the caller's own tenant, resolved from the JWT's tenant claim.</summary>
    public record RemoveTenantLogoCommand : IRequest<ResultDto<TenantDetailsDto>>;
}
